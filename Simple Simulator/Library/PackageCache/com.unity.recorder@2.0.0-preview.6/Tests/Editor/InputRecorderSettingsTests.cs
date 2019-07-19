using UnityEngine;
using NUnit.Framework;
using UnityEditor.Recorder.Input;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder.Tests
{
	class InputRecorderSettingsTests
	{
		[Test]
		public void CameraInputSettings_ShouldHaveProperPublicAPI()
		{
			var input = new CameraInputSettings
			{
				source = ImageSource.MainCamera,
				outputWidth = 640,
				outputHeight = 480,
				cameraTag = "AAA",
				allowTransparency = true,
				captureUI = true,
				flipFinalOutput = false
			};

			Assert.AreEqual(640, input.outputWidth);
			Assert.AreEqual(480, input.outputHeight);
			
			Assert.NotNull(input);
		}

		[Test]
		public void GameViewInputSettings_ShouldHaveProperPublicAPI()
		{
			var input = new GameViewInputSettings
			{
				outputWidth = 1920,
				outputHeight = 1080,
			};
			
			Assert.AreEqual(1920, input.outputWidth);
			Assert.AreEqual(1080, input.outputHeight);

			input.outputWidth = 123;
			input.outputHeight = 456;
			
			Assert.AreEqual(123, input.outputWidth);
			Assert.AreEqual(456, input.outputHeight);
			
			Assert.NotNull(input);
		}
		
		[Test]
		public void Camera360InputSettings_ShouldHaveProperPublicAPI()
		{
			var input = new Camera360InputSettings
			{
				source = ImageSource.MainCamera,
				cameraTag = "AAA",
				renderStereo = true,
				stereoSeparation = 0.065f,
				mapSize = 1024,
				outputWidth = 1024,
				outputHeight = 2048,
				flipFinalOutput = false
			};
			
			Assert.AreEqual(1024, input.outputWidth);
			Assert.AreEqual(2048, input.outputHeight);
			
			Assert.NotNull(input);
		}
		
		[Test]
		public void RenderTextureInputSettings_ShouldHaveProperPublicAPI()
		{
			var rt = new RenderTexture(1234, 123, 24);
			
			var input = new RenderTextureInputSettings
			{
				renderTexture = rt,
				flipFinalOutput = false
			};
			
			Assert.AreEqual(1234, input.outputWidth);
			Assert.AreEqual(123, input.outputHeight);

			input.outputWidth = 256;
			input.outputHeight = 128;
			
			Assert.AreEqual(256, rt.width);
			Assert.AreEqual(128, rt.height);
			
			UnityObject.DestroyImmediate(rt);
			
			Assert.AreEqual(0, input.outputWidth);
			Assert.AreEqual(0, input.outputHeight);
		}
		
		[Test]
		public void RenderTextureSamplerSettings_ShouldHaveProperPublicAPI()
		{
			var input = new RenderTextureSamplerSettings
			{
				source = ImageSource.MainCamera,
				outputWidth = 640,
				outputHeight = 480,
				renderWidth = 1920,
				renderHeight = 1080,
				cameraTag = "AAA",
				colorSpace = ColorSpace.Gamma,
				flipFinalOutput = false,
				superKernelPower = 16.0f,
				superKernelScale = 2.0f,
				superSampling = SuperSamplingCount.X4
			};
			
			Assert.AreEqual(640, input.outputWidth);
			Assert.AreEqual(480, input.outputHeight);
			
			Assert.AreEqual(1920, input.renderWidth);
			Assert.AreEqual(1080, input.renderHeight);
			
			Assert.NotNull(input);
		}
		
		[Test]
		public void AnimationInputSettings_ShouldHaveProperPublicAPI()
		{
			var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

			var input = new AnimationInputSettings { gameObject = cube };
			
			input.AddComponentToRecord(typeof(Transform));
			input.AddComponentToRecord(typeof(Renderer));
			
			Assert.IsTrue(input.gameObject == cube);

			input.gameObject = null;
			
			Assert.IsTrue(input.gameObject == null);
		}
		
		[Test]
		public void AudioInputSettings_ShouldHaveProperPublicAPI()
		{
			var input = new AudioInputSettings
			{
				preserveAudio = true
			};
			
			Assert.NotNull(input);
		}
	}
}
