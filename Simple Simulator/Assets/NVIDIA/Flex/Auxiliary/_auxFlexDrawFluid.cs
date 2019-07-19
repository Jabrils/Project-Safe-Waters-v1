// This code contains NVIDIA Confidential Information and is disclosed to you
// under a form of NVIDIA software license agreement provided separately to you.
//
// Notice
// NVIDIA Corporation and its licensors retain all intellectual property and
// proprietary rights in and to this software and related documentation and
// any modifications thereto. Any use, reproduction, disclosure, or
// distribution of this software and related documentation without an express
// license agreement from NVIDIA Corporation is strictly prohibited.
//
// ALL NVIDIA DESIGN SPECIFICATIONS, CODE ARE PROVIDED "AS IS.". NVIDIA MAKES
// NO WARRANTIES, EXPRESSED, IMPLIED, STATUTORY, OR OTHERWISE WITH RESPECT TO
// THE MATERIALS, AND EXPRESSLY DISCLAIMS ALL IMPLIED WARRANTIES OF NONINFRINGEMENT,
// MERCHANTABILITY, AND FITNESS FOR A PARTICULAR PURPOSE.
//
// Information and code furnished is believed to be accurate and reliable.
// However, NVIDIA Corporation assumes no responsibility for the consequences of use of such
// information or for any infringement of patents or other rights of third parties that may
// result from its use. No license is granted by implication or otherwise under any patent
// or patent rights of NVIDIA Corporation. Details are subject to change without notice.
// This code supersedes and replaces all information previously supplied.
// NVIDIA Corporation products are not authorized for use as critical
// components in life support devices or systems without express written approval of
// NVIDIA Corporation.
//
// Copyright (c) 2018 NVIDIA Corporation. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace NVIDIA.Flex
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("")]
    public class _auxFlexDrawFluid : MonoBehaviour
    {
        #region Messages

        void OnEnable()
        {
            Create();
        }

        void OnDisable()
        {
            Destroy();
        }

        class CameraCommands
        {
            public CommandBuffer copyBackground;
            public List<RenderTexture> fluidDepths = new List<RenderTexture>();
        }
        static Dictionary<Camera, CameraCommands> sm_cameraCommands = new Dictionary<Camera, CameraCommands>();
        static CameraEvent CAMERA_EVENT = CameraEvent.BeforeForwardAlpha;
        static void RemoveCommandBuffer(Camera cam)
        {
            if (sm_cameraCommands.ContainsKey(cam))
            {
                var cameraCommands = sm_cameraCommands[cam];
                if (cameraCommands.fluidDepths.Count > 0)
                {
                    foreach (var cb in cameraCommands.fluidDepths) RenderTexture.ReleaseTemporary(cb);
                    cameraCommands.fluidDepths.Clear();
                }
                else
                {
                    cam.RemoveCommandBuffer(CAMERA_EVENT, cameraCommands.copyBackground);
                    sm_cameraCommands.Remove(cam);
                    if (sm_cameraCommands.Count == 0) Camera.onPostRender -= RemoveCommandBuffer;
                }
            }
        }

        void OnWillRenderObject()
        {
            var cam = Camera.current;

            if (cam.cameraType == CameraType.Preview) return;

            if (!sm_cameraCommands.ContainsKey(cam))
            {
                var copyBackground = new CommandBuffer();
                copyBackground.name = "Copy fluid background";
                int fluidBackgroundID = Shader.PropertyToID("_FluidBackground");
                copyBackground.GetTemporaryRT(fluidBackgroundID, -1, -1, 0);
                copyBackground.Blit(BuiltinRenderTextureType.CurrentActive, fluidBackgroundID);
                cam.AddCommandBuffer(CAMERA_EVENT, copyBackground);
                var cameraCommands = new CameraCommands();
                cameraCommands.copyBackground = copyBackground;
                sm_cameraCommands.Add(cam, cameraCommands);
                if (sm_cameraCommands.Count == 1) Camera.onPostRender += RemoveCommandBuffer;
            }

            int depthPass = m_prepareFluidMaterial.FindPass("FluidDepth".ToUpper());
            int depthBlurPass = m_prepareFluidMaterial.FindPass("FluidDepthBlur".ToUpper());
            if (depthPass != -1 && depthBlurPass != -1 && m_indexBuffer != null)
            {
                RenderTexture active = RenderTexture.active;

                // Depth texture
                RenderTextureDescriptor depthDesc = new RenderTextureDescriptor(cam.pixelWidth, cam.pixelHeight);
                depthDesc.colorFormat = RenderTextureFormat.RGFloat;
                depthDesc.depthBufferBits = 24;
                RenderTexture depth = RenderTexture.GetTemporary(depthDesc);
                Graphics.SetRenderTarget(depth);
                GL.Clear(true, true, new Color(cam.farClipPlane, cam.farClipPlane, 0, 0), 1.0f);
                m_prepareFluidMaterial.SetBuffer("_Indices", m_indexBuffer);
                m_prepareFluidMaterial.SetBuffer("_Positions", m_positionBuffer);
                m_prepareFluidMaterial.SetBuffer("_Anisotropy1", m_anisotropy1Buffer);
                m_prepareFluidMaterial.SetBuffer("_Anisotropy2", m_anisotropy2Buffer);
                m_prepareFluidMaterial.SetBuffer("_Anisotropy3", m_anisotropy3Buffer);
                if (cam.stereoActiveEye != Camera.MonoOrStereoscopicEye.Mono)
                {
                    m_prepareFluidMaterial.SetMatrixArray("_ViewMatrix", new[] { cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left), cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right) });
                    m_prepareFluidMaterial.SetMatrixArray("_ProjMatrix", new[] { GL.GetGPUProjectionMatrix(cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left), active != null), GL.GetGPUProjectionMatrix(cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right), active != null) });
                    m_prepareFluidMaterial.SetInt("_EyeCount", 2);
                    m_prepareFluidMaterial.SetPass(depthPass);
                    Graphics.DrawProceduralNow(MeshTopology.Points, m_indexBuffer.count);
                }
                else
                {
                    m_prepareFluidMaterial.SetMatrixArray("_ViewMatrix", new[] { cam.worldToCameraMatrix, Matrix4x4.identity });
                    m_prepareFluidMaterial.SetMatrixArray("_ProjMatrix", new[] { GL.GetGPUProjectionMatrix(cam.projectionMatrix, active != null), Matrix4x4.identity });
                    m_prepareFluidMaterial.SetInt("_EyeCount", 1);
                    m_prepareFluidMaterial.SetPass(depthPass);
                    Graphics.DrawProceduralNow(MeshTopology.Points, m_indexBuffer.count);
                }
                Graphics.SetRenderTarget(active);

                // Blur texture
                RenderTextureDescriptor depthBlurDesc = new RenderTextureDescriptor(cam.pixelWidth, cam.pixelHeight);
                depthBlurDesc.colorFormat = RenderTextureFormat.RGFloat;
                depthBlurDesc.depthBufferBits = 0;
                RenderTexture depthBlur = RenderTexture.GetTemporary(depthBlurDesc);
                Graphics.SetRenderTarget(depthBlur);
                GL.Clear(false, true, new Color(cam.farClipPlane, cam.farClipPlane, 0, 0));
                m_prepareFluidMaterial.SetFloat("_FarPlane", cam.farClipPlane);
                m_prepareFluidMaterial.SetVector("_InvScreen", new Vector2(1.0f / cam.pixelWidth, 1.0f / cam.pixelHeight));
                m_prepareFluidMaterial.SetTexture("_DepthTex", depth);
                Graphics.Blit(null, m_prepareFluidMaterial, depthBlurPass);

                RenderTexture.ReleaseTemporary(depth);
                sm_cameraCommands[cam].fluidDepths.Add(depthBlur);

                m_fluidMaterial.SetTexture("_DepthTex", depthBlur);
                m_fluidMaterial.SetFloat("FLEX_FLIP_Y", active ? 1.0f : 0.0f);
            }
        }

        #endregion

        #region Private

        const string PREPARE_FLUID_SHADER = "Flex/FlexPrepareFluid";
        const string DRAW_FLUID_SHADER = "Flex/FlexDrawFluid2";

        void Create()
        {
            m_scene = GetComponentInParent<FlexScene>();
            if (m_scene == null)
            {
                Debug.LogError("_auxFlexDrawFluid should be parented to a FlexScene");
                Debug.Break();
            }

            if (m_scene && m_scene.container && m_scene.container.handle)
            {
                m_mesh = new Mesh();
                m_mesh.name = "Flex Fluid";
                m_mesh.vertices = new Vector3[1];
                m_mesh.SetIndices(new int[0], MeshTopology.Points, 0);

                m_prepareFluidMaterial = new Material(Shader.Find(PREPARE_FLUID_SHADER));
                m_prepareFluidMaterial.hideFlags = HideFlags.HideAndDontSave;

                m_fluidMaterial = m_scene.container.fluidMaterial;

                MeshFilter meshFilter = GetComponent<MeshFilter>();
                meshFilter.mesh = m_mesh;

                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.material = m_fluidMaterial;
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                meshRenderer.receiveShadows = true;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetSelectedRenderState(meshRenderer, UnityEditor.EditorSelectedRenderState.Hidden);
#endif

                int maxParticles = m_scene.container.maxParticles;

                m_positionBuffer = new ComputeBuffer(maxParticles, sizeof(float) * 4);
                m_anisotropy1Buffer = new ComputeBuffer(maxParticles, sizeof(float) * 4);
                m_anisotropy2Buffer = new ComputeBuffer(maxParticles, sizeof(float) * 4);
                m_anisotropy3Buffer = new ComputeBuffer(maxParticles, sizeof(float) * 4);

                m_positionBuffer0 = Flex.AllocBuffer(FlexContainer.library, maxParticles, sizeof(float) * 4, Flex.BufferType.Host);
                m_anisotropy1Buffer0 = Flex.AllocBuffer(FlexContainer.library, maxParticles, sizeof(float) * 4, Flex.BufferType.Host);
                m_anisotropy2Buffer0 = Flex.AllocBuffer(FlexContainer.library, maxParticles, sizeof(float) * 4, Flex.BufferType.Host);
                m_anisotropy3Buffer0 = Flex.AllocBuffer(FlexContainer.library, maxParticles, sizeof(float) * 4, Flex.BufferType.Host);

                m_temporaryBuffer = new Vector4[maxParticles];
            }
        }

        void Destroy()
        {
            if (m_mesh) DestroyImmediate(m_mesh);
            if (m_prepareFluidMaterial) DestroyImmediate(m_prepareFluidMaterial);
            if (m_indexBuffer != null) m_indexBuffer.Release();

            if (m_positionBuffer != null) { m_positionBuffer.Release(); m_positionBuffer = null; }
            if (m_anisotropy1Buffer != null) { m_anisotropy1Buffer.Release(); m_anisotropy1Buffer = null; }
            if (m_anisotropy2Buffer != null) { m_anisotropy2Buffer.Release(); m_anisotropy2Buffer = null; }
            if (m_anisotropy3Buffer != null) { m_anisotropy3Buffer.Release(); m_anisotropy3Buffer = null; }

            if (m_positionBuffer0) { Flex.FreeBuffer(m_positionBuffer0); m_positionBuffer0.Clear(); }
            if (m_anisotropy1Buffer0) { Flex.FreeBuffer(m_anisotropy1Buffer0); m_anisotropy1Buffer0.Clear(); }
            if (m_anisotropy2Buffer0) { Flex.FreeBuffer(m_anisotropy2Buffer0); m_anisotropy2Buffer0.Clear(); }
            if (m_anisotropy3Buffer0) { Flex.FreeBuffer(m_anisotropy3Buffer0); m_anisotropy3Buffer0.Clear(); }
        }

        public void UpdateMesh(FlexContainer.ParticleData _particleData)
        {
            transform.rotation = Quaternion.identity;

            if (m_scene && m_scene.container && m_fluidMaterial)
            {
                int[] indices = m_scene.container.fluidIndices;
                int indexCount = m_scene.container.fluidIndexCount;
                if (m_indexBuffer != null && indexCount != m_indexBuffer.count) // @@@
                {
                    m_indexBuffer.Release();
                    m_indexBuffer = null;
                }
                if (m_indexBuffer == null && indexCount > 0)
                {
                    m_indexBuffer = new ComputeBuffer(indexCount, sizeof(int));
                    m_mesh.SetIndices(new int[indexCount], MeshTopology.Points, 0);
                }
                if (m_indexBuffer != null)
                {
                    m_indexBuffer.SetData(indices);
                }
                Vector3 boundsMin = Vector3.one * 1e10f, boundsMax = Vector3.one * 1e10f;
                if (indexCount > 0)
                {
                    int maxParticles = m_scene.container.maxParticles;

                    Flex.GetParticles(m_scene.container.solver, m_positionBuffer0);
                    Flex.GetAnisotropy(m_scene.container.solver, m_anisotropy1Buffer0, m_anisotropy2Buffer0, m_anisotropy3Buffer0);

                    CopyBufferContent(m_positionBuffer0, m_positionBuffer);
                    CopyBufferContent(m_anisotropy1Buffer0, m_anisotropy1Buffer);
                    CopyBufferContent(m_anisotropy2Buffer0, m_anisotropy2Buffer);
                    CopyBufferContent(m_anisotropy3Buffer0, m_anisotropy3Buffer);

                    m_fluidMaterial.SetBuffer("_Points", m_positionBuffer);
                    m_fluidMaterial.SetBuffer("_Anisotropy1", m_anisotropy1Buffer);
                    m_fluidMaterial.SetBuffer("_Anisotropy2", m_anisotropy2Buffer);
                    m_fluidMaterial.SetBuffer("_Anisotropy3", m_anisotropy3Buffer);
                    m_fluidMaterial.SetBuffer("_Indices", m_indexBuffer);

                    FlexUtils.ComputeBounds(_particleData.particleData.particles, ref indices[0], indices.Length, ref boundsMin, ref boundsMax);
                }
                Vector3 center = (boundsMin + boundsMax) * 0.5f;
                Vector3 size = boundsMax - boundsMin;
                m_bounds.center = Vector3.zero;
                m_bounds.size = size;
                m_bounds.Expand(m_scene.container.radius);
                m_mesh.bounds = m_bounds;
                transform.position = center;
            }
        }

        void CopyBufferContent(Flex.Buffer source, ComputeBuffer target)
        {
            IntPtr ptr = Flex.Map(source, Flex.MapFlags.Wait);
            FlexUtils.FastCopy(ptr, m_temporaryBuffer);
            target.SetData(m_temporaryBuffer);
            Flex.Unmap(source);
        }

        FlexScene m_scene;
        Mesh m_mesh;
        Bounds m_bounds = new Bounds();

        [NonSerialized]
        Material m_fluidMaterial;
        [NonSerialized]
        Material m_prepareFluidMaterial;
        [NonSerialized]
        ComputeBuffer m_positionBuffer, m_anisotropy1Buffer, m_anisotropy2Buffer, m_anisotropy3Buffer, m_indexBuffer;
        [NonSerialized]
        Flex.Buffer m_positionBuffer0, m_anisotropy1Buffer0, m_anisotropy2Buffer0, m_anisotropy3Buffer0;
        [NonSerialized]
        Vector4[] m_temporaryBuffer = new Vector4[0];

        #endregion
    }
}
