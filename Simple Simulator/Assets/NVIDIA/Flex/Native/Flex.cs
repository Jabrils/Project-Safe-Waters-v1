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
using System.Runtime.InteropServices;
using UnityEngine;

namespace NVIDIA.Flex
{
    public static class Flex
    {
#if FLEX_CUDA
#   if FLEX_DEBUG
        const string FLEX_DLL = "NvFlexDebugCUDA_x64";
#   else
        const string FLEX_DLL = "NvFlexReleaseCUDA_x64";
#   endif
#else
#   if FLEX_DEBUG
        const string FLEX_DLL = "NvFlexDebugD3D_x64";
#   else
        const string FLEX_DLL = "NvFlexReleaseD3D_x64";
#   endif
#endif

        // least 2 significant digits define minor version, eg: 10 -> version 0.10
        public const int FLEX_VERSION = 120;

        /**
         * Opaque type representing a library that can create FlexSolvers, FlexTriangleMeshes, and FlexBuffers
         */
        public struct Library { IntPtr _; static public implicit operator bool(Library _) { return _._ != default(IntPtr); } public void Clear() { _ = default(IntPtr); } }

        /**
         * Opaque type representing a collection of particles and constraints
         */
        public struct Solver { IntPtr _; static public implicit operator bool(Solver _) { return _._ != default(IntPtr); } public void Clear() { _ = default(IntPtr); } }

        /**
         * Opaque type representing a data buffer, type and contents depends on usage, see flexAllocBuffer()
         */
        public struct Buffer { IntPtr _; static public implicit operator bool(Buffer _) { return _._ != default(IntPtr); } public void Clear() { _ = default(IntPtr); } }

        /**
         * Controls behavior of NvFlexMap()
         */
        public enum MapFlags
        {
            Wait = 0,         //!< Calling thread will be blocked until buffer is ready for access, default
            DoNotWait = 1,    //!< Calling thread will check if buffer is ready for access, if not ready then the method will return NULL immediately
        };

        /**
         * Controls memory space of a NvFlexBuffer, see NvFlexAllocBuffer()
         */
        public enum BufferType
        {
            Host = 0,      //!< Host mappable buffer, pinned memory on CUDA, staging buffer on DX
            Device = 1,    //!< Device memory buffer, mapping this on CUDA will return a device memory pointer, and will return a buffer pointer on DX
        };

        /**
         * Controls the relaxation method used by the solver to ensure convergence
         */
        public enum RelaxationMode
        {
            Global = 0,    //!< The relaxation factor is a fixed multiplier on each constraint's position delta
            Local = 1      //!< The relaxation factor is a fixed multiplier on each constraint's delta divided by the particle's constraint count, convergence will be slower but more reliable
        };

        /**
         * Simulation parameters for a solver
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct Params
        {
            public int numIterations;                  //!< Number of solver iterations to perform per-substep

            public Vector3 gravity;                    //!< Constant acceleration applied to all particles
            public float radius;                       //!< The maximum interaction radius for particles
            public float solidRestDistance;            //!< The distance non-fluid particles attempt to maintain from each other, must be in the range (0, radius]
            public float fluidRestDistance;            //!< The distance fluid particles are spaced at the rest density, must be in the range (0, radius], for fluids this should generally be 50-70% of mRadius, for rigids this can simply be the same as the particle radius

            // common params
            public float dynamicFriction;              //!< Coefficient of friction used when colliding against shapes
            public float staticFriction;               //!< Coefficient of static friction used when colliding against shapes
            public float particleFriction;             //!< Coefficient of friction used when colliding particles
            public float restitution;                  //!< Coefficient of restitution used when colliding against shapes, particle collisions are always inelastic
            public float adhesion;                     //!< Controls how strongly particles stick to surfaces they hit, default 0.0, range [0.0, +inf]
            public float sleepThreshold;               //!< Particles with a velocity magnitude < this threshold will be considered fixed

            public float maxSpeed;                     //!< The magnitude of particle velocity will be clamped to this value at the end of each step
            public float maxAcceleration;              //!< The magnitude of particle acceleration will be clamped to this value at the end of each step (limits max velocity change per-second), useful to avoid popping due to large interpenetrations

            public float shockPropagation;             //!< Artificially decrease the mass of particles based on height from a fixed reference point, this makes stacks and piles converge faster
            public float dissipation;                  //!< Damps particle velocity based on how many particle contacts it has
            public float damping;                      //!< Viscous drag force, applies a force proportional, and opposite to the particle velocity

            // cloth params
            public Vector3 wind;                       //!< Constant acceleration applied to particles that belong to dynamic triangles, drag needs to be > 0 for wind to affect triangles
            public float drag;                         //!< Drag force applied to particles belonging to dynamic triangles, proportional to velocity^2*area in the negative velocity direction
            public float lift;                         //!< Lift force applied to particles belonging to dynamic triangles, proportional to velocity^2*area in the direction perpendicular to velocity and (if possible), parallel to the plane normal

            // fluid params
            public float cohesion;                     //!< Control how strongly particles hold each other together, default: 0.025, range [0.0, +inf]
            public float surfaceTension;               //!< Controls how strongly particles attempt to minimize surface area, default: 0.0, range: [0.0, +inf]    
            public float viscosity;                    //!< Smoothes particle velocities using XSPH viscosity
            public float vorticityConfinement;         //!< Increases vorticity by applying rotational forces to particles
            public float anisotropyScale;              //!< Control how much anisotropy is present in resulting ellipsoids for rendering, if zero then anisotropy will not be calculated, see NvFlexGetAnisotropy()
            public float anisotropyMin;                //!< Clamp the anisotropy scale to this fraction of the radius
            public float anisotropyMax;                //!< Clamp the anisotropy scale to this fraction of the radius
            public float smoothing;                    //!< Control the strength of Laplacian smoothing in particles for rendering, if zero then smoothed positions will not be calculated, see NvFlexGetSmoothParticles()
            public float solidPressure;                //!< Add pressure from solid surfaces to particles
            public float freeSurfaceDrag;              //!< Drag force applied to boundary fluid particles
            public float buoyancy;                     //!< Gravity is scaled by this value for fluid particles

            // diffuse params
            public float diffuseThreshold;             //!< Particles with kinetic energy + divergence above this threshold will spawn new diffuse particles
            public float diffuseBuoyancy;              //!< Scales force opposing gravity that diffuse particles receive
            public float diffuseDrag;                  //!< Scales force diffuse particles receive in direction of neighbor fluid particles
            public int diffuseBallistic;               //!< The number of neighbors below which a diffuse particle is considered ballistic
            public float diffuseLifetime;              //!< Time in seconds that a diffuse particle will live for after being spawned, particles will be spawned with a random lifetime in the range [0, diffuseLifetime]

            // collision params
            public float collisionDistance;            //!< Distance particles maintain against shapes, note that for robust collision against triangle meshes this distance should be greater than zero
            public float particleCollisionMargin;      //!< Increases the radius used during neighbor finding, this is useful if particles are expected to move significantly during a single step to ensure contacts aren't missed on subsequent iterations
            public float shapeCollisionMargin;         //!< Increases the radius used during contact finding against kinematic shapes

            public Vector4 plane0, plane1, plane2, plane3, plane4, plane5, plane6, plane7;  //!< Collision planes in the form ax + by + cz + d = 0
            public int numPlanes;                      //!< Num collision planes

            public RelaxationMode relaxationMode;       //!< How the relaxation is applied inside the solver
            public float relaxationFactor;              //!< Control the convergence rate of the parallel solver, default: 1, values greater than 1 may lead to instability
        };

        /**
         * Flags that control the a particle's behavior and grouping, use NvFlexMakePhase() to construct a valid 32bit phase identifier
         */
        public enum Phase
        {
            Default = 0,                        //!< No flags set. Particles of the same group won't collide

            GroupMask = 0x000fffff,             //!< Bits [ 0, 19] represent the particle group for controlling collisions
            FlagsMask = 0x00f00000,             //!< Bits [20, 23] hold flags about how the particle behave 
            ShapeChannelMask = -16777216/*0xff000000*/,      //!< Bits [24, 31] hold flags representing what shape collision channels particles will collide with, see NvFlexMakeShapeFlags()

            SelfCollide = 1 << 20,             //!< If set this particle will interact with particles of the same group
            SelfCollideFilter = 1 << 21,       //!< If set this particle will ignore collisions with particles closer than the radius in the rest pose, this flag should not be specified unless valid rest positions have been specified using NvFlexSetRestParticles()
            Fluid = 1 << 22,                   //!< If set this particle will generate fluid density constraints for its overlapping neighbors
            Unused = 1 << 23,                  //!< Reserved

            ShapeChannel0 = 1 << 24,           //!< Particle will collide with shapes with channel 0 set (see NvFlexMakeShapeFlags())
            ShapeChannel1 = 1 << 25,           //!< Particle will collide with shapes with channel 1 set (see NvFlexMakeShapeFlags())
            ShapeChannel2 = 1 << 26,           //!< Particle will collide with shapes with channel 2 set (see NvFlexMakeShapeFlags())
            ShapeChannel3 = 1 << 27,           //!< Particle will collide with shapes with channel 3 set (see NvFlexMakeShapeFlags())
            ShapeChannel4 = 1 << 28,           //!< Particle will collide with shapes with channel 4 set (see NvFlexMakeShapeFlags())
            ShapeChannel5 = 1 << 29,           //!< Particle will collide with shapes with channel 5 set (see NvFlexMakeShapeFlags())
            ShapeChannel6 = 1 << 30,           //!< Particle will collide with shapes with channel 6 set (see NvFlexMakeShapeFlags())
            ShapeChannel7 = 1 << 31,		    //!< Particle will collide with shapes with channel 7 set (see NvFlexMakeShapeFlags())
        };



        /**
         * Generate a bit set for the particle phase, this is a helper method to simply combine the
         * group id and bit flags into a single integer.
         *
         * @param[in] group The index of the group for this particle, should be an integer < 2^20
         * @param[in] particleFlags A combination of the phase flags which should be a combination of eNvFlexPhaseSelfCollide, eNvFlexPhaseSelfCollideFilter, and eNvFlexPhaseFluid
         * @param[in] shapeChannels A combination of eNvFlexPhaseShapeChannel* flags that control which shapes will be collided against, particles will only collide against shapes that share at least one set channel, see NvFlexMakeShapeFlagsWithChannels()
         */
        public static int MakePhaseWithChannels(int group, Phase particleFlags, Phase shapeChannels) { return (group & (int)Phase.GroupMask) | ((int)particleFlags & (int)Phase.FlagsMask) | ((int)shapeChannels & (int)Phase.ShapeChannelMask); }

        /**
         * Deprecated helper method to generates a phase with all shape channels set
         */
        public static int MakePhase(int group, Phase particleFlags) { return MakePhaseWithChannels(group, particleFlags, Phase.ShapeChannelMask); }


        ///**
        // * Generate a bit set for the particle phase, the group should be an integer < 2^24, and the flags should be a combination of FlexPhase enum values
        // */
        //public static int MakePhase(int group, Phase flags) { return (group & (int)Phase.GroupMask) | (int)flags; }

        /**
         * Time spent in each section of the solver update, times in GPU seconds, see NvFlexUpdateSolver()
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct Timers
        {
            public float predict;              //!< Time spent in prediction
            public float createCellIndices;    //!< Time spent creating grid indices
            public float sortCellIndices;      //!< Time spent sorting grid indices
            public float createGrid;           //!< Time spent creating grid
            public float reorder;              //!< Time spent reordering particles
            public float collideParticles;     //!< Time spent finding particle neighbors
            public float collideShapes;        //!< Time spent colliding convex shapes
            public float collideTriangles;     //!< Time spent colliding triangle shapes
            public float collideFields;        //!< Time spent colliding signed distance field shapes
            public float calculateDensity;     //!< Time spent calculating fluid density
            public float solveDensities;       //!< Time spent solving density constraints
            public float solveVelocities;      //!< Time spent solving velocity constraints
            public float solveShapes;          //!< Time spent solving rigid body constraints
            public float solveSprings;         //!< Time spent solving distance constraints
            public float solveContacts;        //!< Time spent solving contact constraints
            public float solveInflatables;     //!< Time spent solving pressure constraints
            public float applyDeltas;          //!< Time spent adding position deltas to particles
            public float calculateAnisotropy;  //!< Time spent calculating particle anisotropy for fluid
            public float updateDiffuse;        //!< Time spent updating diffuse particles
            public float updateTriangles;      //!< Time spent updating dynamic triangles
            public float updateNormals;        //!< Time spent updating vertex normals
            public float finalize;             //!< Time spent finalizing state
            public float updateBounds;         //!< Time spent updating particle bounds
            public float total;                //!< Sum of all timers above
        };

        /**
         * Flex error return codes
         */
        public enum ErrorSeverity
        {
            LogError = 0,   //!< Error messages
            LogInfo = 1,    //!< Information messages
            LogWarning = 2, //!< Warning messages
            LogDebug = 4,   //!< Used only in debug version of dll
            LogAll = -1,    //!< All log types
        };

        /**
         * Defines the set of stages at which callbacks may be registered 
         */
        public enum SolverCallbackStage
        {
            IterationStart, //!< Called at the beginning of each constraint iteration
            IterationEnd,   //!< Called at the end of each constraint iteration
            SubstepBegin,   //!< Called at the beginning of each substep after the prediction step has been completed
            SubstepEnd,     //!< Called at the end of each substep after the velocity has been updated by the constraints
            UpdateEnd,      //!< Called at the end of solver update after the final substep has completed
            Count,          //!< Number of stages
        };

        /**
         *  Structure containing pointers to the internal solver data that is passed to each registered solver callback
         *
         *  @remarks Pointers to internal data are only valid for the lifetime of the callback and should not be stored.
         *  However, it is safe to launch kernels and memory transfers using the device pointers.
         *
         *  @remarks Because Flex re-orders particle data internally for performance, the particle data in the callback is not
         *  in the same order as it was provided to the API. The callback provides arrays which map original particle indices
         *  to sorted positions and vice-versa.
         *
         *  @remarks Particle positions may be modified during any callback, but velocity modifications should only occur during 
         *  the eNvFlexStageUpdateEnd stage, otherwise any velocity changes will be discarded.
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct SolverCallbackParams
        {
            public Solver solver;                  //!< Pointer to the solver that the callback is registered to
            public IntPtr userData;                //!< Pointer to the user data provided to NvFlexRegisterSolverCallback()

            public IntPtr particles;               //!< Device pointer to the active particle basic data in the form x,y,z,1/m
            public IntPtr velocities;              //!< Device pointer to the active particle velocity data in the form x,y,z,w (last component is not used)
            public IntPtr phases;                  //!< Device pointer to the active particle phase data

            public int numActive;                  //!< The number of active particles returned, the callback data only return pointers to active particle data, this is the same as NvFlexGetActiveCount()

            public float dt;                       //!< The per-update time-step, this is the value passed to NvFlexUpdateSolver()

            public IntPtr originalToSortedMap;     //!< Device pointer that maps the sorted callback data to the original position given by SetParticles()
            public IntPtr sortedToOriginalMap;     //!< Device pointer that maps the original particle index to the index in the callback data structure
        };

        /**
         *  Solver callback definition, see NvFlexRegisterSolverCallback()
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct SolverCallback
        {
            /** User data passed to the callback*/
            public IntPtr userData;

            /** Function pointer to a callback method */
            public delegate void Callback(SolverCallbackParams parameters);
            public Callback function;
        };

        /**
         * Function pointer type for error reporting callbacks
         */
        public delegate void ErrorCallback(ErrorSeverity type, IntPtr msg, IntPtr file, int line);

        /**
         *  Defines the different DirectX compute modes that Flex can use
         */
        public enum ComputeType
        {
            CUDA,        //!< Use CUDA compute for Flex, the application must link against the CUDA libraries
            D3D11,       //!< Use DirectX 11 compute for Flex, the application must link against the D3D libraries
            D3D12,       //!< Use DirectX 12 compute for Flex, the application must link against the D3D libraries
        };

        /**
         *  Descriptor used to initialize Flex
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct InitDesc
        {
            public int deviceIndex;             //!< The GPU device index that should be used, if there is already a CUDA context on the calling thread then this parameter will be ignored and the active CUDA context used. Otherwise a new context will be created using the suggested device ordinal.
            public bool enableExtensions;       //!< Enable or disable NVIDIA/AMD extensions in DirectX, can lead to improved performance.
            public IntPtr renderDevice;         //!< Direct3D device to use for simulation, if none is specified a new device and context will be created.
            public IntPtr renderContext;        //!< Direct3D context to use for simulation, if none is specified a new context will be created, in DirectX 12 this should be a pointer to the ID3D12CommandQueue where compute operations will take place. 
            public IntPtr computeContext;       //!< Direct3D context to use for simulation, if none is specified a new context will be created, in DirectX 12 this should be a pointer to the ID3D12CommandQueue where compute operations will take place. 
            public bool runOnRenderContext;		//!< If true, run Flex on D3D11 render context, or D3D12 direct queue. If false, run on a D3D12 compute queue, or vendor specific D3D11 compute queue, allowing compute and graphics to run in parallel on some GPUs.

            public ComputeType computeType;     //!< Set to eNvFlexD3D11 if DirectX 11 should be used, eNvFlexD3D12 for DirectX 12, this must match the libraries used to link the application
        };

        /**
         * Initialize library, should be called before any other API function.
         *
         * @param[in] version The version number the app is expecting, should almost always be NV_FLEX_VERSION
         * @param[in] errorFunc The callback used for reporting errors.
         * @param[in] desc The NvFlexInitDesc struct defining the device ordinal, D3D device/context and the type of D3D compute being used
         * @return A pointer to a library instance that can be used to allocate shared object such as triangle meshes, buffers, etc
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexInit")]
        public static extern Library Init(int version = FLEX_VERSION, ErrorCallback errorFunc = null, IntPtr desc = default(IntPtr));
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexInit")]
        public static extern Library Init(int version, ErrorCallback errorFunc, ref InitDesc desc);

        /**
         * Shutdown library, users should manually destroy any previously created   
         * solvers to ensure memory is freed before calling this method. If a new CUDA context was created during NvFlexInit() then it will be destroyed.
         *
         * @param[in] lib The library intance to use
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexShutdown")]
        public static extern void Shutdown(Library lib);

        /**
         * Get library version number
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetVersion")]
        public static extern int GetVersion();



        /** 
         * Controls which features are enabled, choosing a simple option will disable features and can lead to better performance and reduced memory usage
         */
        public enum FeatureMode
        {
            Default = 0,        //!< All features enabled
            SimpleSolids = 1,   //!< Simple per-particle collision (no per-particle SDF normals, no fluids)
            SimpleFluids = 2,   //!< Simple single phase fluid-only particles (no solids)
        };

        /**
         * Describes the creation time parameters for the solver
         */
        public struct SolverDesc
        {
            FeatureMode featureMode;  //!< Control which features are enabled

            public int maxParticles;               //!< Maximum number of regular particles in the solver
            public int maxDiffuseParticles;        //!< Maximum number of diffuse particles in the solver
            public int maxNeighborsPerParticle;    //!< Maximum number of neighbors per-particle, for solids this can be around 32, for fluids up to 128 may be necessary depending on smoothing radius
            public int maxContactsPerParticle;     //!< Maximum number of collision contacts per-particle
        };

        /**
         * Initialize the solver desc to its default values
         * @param[in] desc Pointer to a description structure that will be initialized to default values
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetSolverDescDefaults")]
        public static extern void SetSolverDescDefaults(ref SolverDesc desc);

        /**
         * Create a new particle solver
         *
         * @param[in] lib The library instance to use
         * @param[in] desc Pointer to a solver description structure used to create the solver
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexCreateSolver")]
        public static extern Solver CreateSolver(Library lib, ref SolverDesc desc);

        /**
         * Delete a particle solver
         *
         * @param[in] solver A valid solver pointer created from NvFlexCreateSolver()
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexDestroySolver")]
        public static extern void DestroySolver(Solver solver);

        /**
         * Return the library associated with a solver
         *
         * @param[in] solver A valid solver created with NvFlexCreateSolver()
         * @return A library pointer
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetSolverLibrary")]
        public static extern Library GetSolverLibrary(Solver solver);

        /**
         * Return the solver desc that was used to create a solver
         *
         * @param[in] solver Pointer to a valid Flex solver
         * @param[in] desc Pointer to a desc structure
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetSolverDesc")]
        public static extern void GetSolverDesc(Solver solver, ref SolverDesc desc);

        /**
         *  Registers a callback for a solver stage, the callback will be invoked from the same thread that calls NvFlexUpdateSolver().
         *
         * @param[in] solver A valid solver
         * @param[in] function A pointer to a function that will be called during the solver update
         * @param[in] stage The stage of the update at which the callback function will be called
         *
         * @return The previously registered callback for this slot, this allows multiple users to chain callbacks together
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexRegisterSolverCallback")]
        public static extern SolverCallback RegisterSolverCallback(Solver solver, SolverCallback function, SolverCallbackStage stage);

        /**
         * Integrate particle solver forward in time. Below is an example of how to step Flex in the context of a simple game loop:
         *
         * @param[in] solver A valid solver
         * @param[in] dt Time to integrate the solver forward in time by
         * @param[in] substeps The time dt will be divided into the number of sub-steps given by this parameter
         * @param[in] enableTimers Whether to enable per-kernel timers for profiling. Note that profiling can substantially slow down overall performance so this param should only be true in non-release builds
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexUpdateSolver")]
        public static extern void UpdateSolver(Solver solver, float dt, int substeps, bool enableTimers = false);

        /**
         * Update solver paramters
         *
         * @param[in] solver A valid solver
         * @param[in] params Parameters structure in host memory, see NvFlexParams
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetParams")]
        public static extern void SetParams(Solver solver, ref Params parameters);

        /**
         * Retrieve solver paramters, default values will be set at solver creation time
         *
         * @param[in] solver A valid solver
         * @param[out] params Parameters structure in host memory, see NvFlexParams
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetParams")]
        public static extern void GetParams(Solver solver, ref Params parameters);

        /**
         * Describes a source and destination buffer region for performing a copy operation.
         */
        public struct CopyDesc
        {
            public int srcOffset;          //<! Offset in elements from the start of the source buffer to begin reading from
            public int dstOffset;          //<! Offset in elements from the start of the destination buffer to being writing to
            public int elementCount;       //<! Number of elements to copy
        };

        /**
         * Set the active particles indices in the solver
         * 
         * @param[in] solver A valid solver
         * @param[in] indices Holds the indices of particles that have been made active
         * @param[in] desc Describes the copy region, if NULL the solver will try to access the entire buffer (maxParticles length)
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetActive")]
        public static extern void SetActive(Solver solver, Buffer indices, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetActive")]
        public static extern void SetActive(Solver solver, Buffer indices, IntPtr desc = default(IntPtr));

        /**
         * Return the active particle indices
         * 
         * @param[in] solver A valid solver
         * @param[out] indices a buffer of indices at least activeCount in length
         * @param[in] desc Describes the copy region, if NULL the solver will try to access the entire buffer (maxParticles length)
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetActive")]
        public static extern void GetActive(Solver solver, Buffer indices, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetActive")]
        public static extern void GetActive(Solver solver, Buffer indices, IntPtr desc = default(IntPtr));

        /**
         * Set the total number of active particles
         * 
         * @param[in] solver A valid solver
         * @param[in] n The number of active particles, the first n indices in the active particles array will be used as the active count
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetActiveCount")]
        public static extern void SetActiveCount(Solver solver, int n);

        /**
         * Return the number of active particles in the solver
         * 
         * @param[in] solver A valid solver
         * @return The number of active particles in the solver
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetActiveCount")]
        public static extern int GetActiveCount(Solver solver);

        /**
         * Set the particles state of the solver, a particle consists of 4 floating point numbers, its x,y,z position followed by its inverse mass (1/m)
         * 
         * @param[in] solver A valid solver
         * @param[in] p Pointer to a buffer of particle data, should be 4*n in length
         * @param[in] desc Describes the copy region, if NULL the solver will try to access the entire buffer (maxParticles length)
         *
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetParticles")]
        public static extern void SetParticles(Solver solver, Buffer p, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetParticles")]
        public static extern void SetParticles(Solver solver, Buffer p, IntPtr desc = default(IntPtr));

        /**
         * Get the particles state of the solver, a particle consists of 4 floating point numbers, its x,y,z position followed by its inverse mass (1/m)
         * 
         * @param[in] solver A valid solver
         * @param[out] p Pointer to a buffer of 4*n floats that will be filled out with the particle data, can be either a host or device pointer
         * @param[in] desc Describes the copy region, if NULL the solver will try to access the entire buffer (maxParticles length)
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetParticles")]
        public static extern void GetParticles(Solver solver, Buffer p, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetParticles")]
        public static extern void GetParticles(Solver solver, Buffer p, IntPtr desc = default(IntPtr));

        /**
         * Set the particle positions in their rest state, if eNvFlexPhaseSelfCollideFilter is set on the particle's
         * phase attribute then particles that overlap in the rest state will not generate collisions with each other
         * 
         * @param[in] solver A valid solver
         * @param[in] p Pointer to a buffer of particle data, should be 4*n in length
         * @param[in] desc Describes the copy region, if NULL the solver will try to access the entire buffer (maxParticles length)
         *
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetRestParticles")]
        public static extern void SetRestParticles(Solver solver, Buffer p, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetRestParticles")]
        public static extern void SetRestParticles(Solver solver, Buffer p, IntPtr desc = default(IntPtr));

        /**
         * Get the particle positions in their rest state
         * 
         * @param[in] solver A valid solver
         * @param[in] p Pointer to a buffer of particle data, should be 4*n in length
         * @param[in] desc Describes the copy region, if NULL the solver will try to access the entire buffer (maxParticles length)
         *
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetRestParticles")]
        public static extern void GetRestParticles(Solver solver, Buffer p, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetRestParticles")]
        public static extern void GetRestParticles(Solver solver, Buffer p, IntPtr desc = default(IntPtr));

        /**
         * Get the Laplacian smoothed particle positions for rendering, see NvFlexParams::smoothing
         * 
         * @param[in] solver A valid solver
         * @param[out] p Pointer to a buffer of 4*n floats that will be filled out with the data, can be either a host or device pointer
         * @param[in] desc Describes the copy region, if NULL the solver will try to access the entire buffer (maxParticles length)
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetSmoothParticles")]
        public static extern void GetSmoothParticles(Solver solver, Buffer p, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetSmoothParticles")]
        public static extern void GetSmoothParticles(Solver solver, Buffer p, IntPtr desc = default(IntPtr));

        /**
         * Set the particle velocities, each velocity is a 3-tuple of x,y,z floating point values
         * 
         * @param[in] solver A valid solver
         * @param[in] v Pointer to a buffer of 3*n floats
         * @param[in] desc Describes the copy region, if NULL the solver will try to access the entire buffer (maxParticles length)
         *
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetVelocities")]
        public static extern void SetVelocities(Solver solver, Buffer v, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetVelocities")]
        public static extern void SetVelocities(Solver solver, Buffer v, IntPtr desc = default(IntPtr));

        /**
         * Get the particle velocities, each velocity is a 3-tuple of x,y,z floating point values
         * 
         * @param[in] solver A valid solver
         * @param[out] v Pointer to a buffer of 3*n floats that will be filled out with the data, can be either a host or device pointer
         * @param[in] desc Describes the copy region, if NULL the solver will try to access the entire buffer (maxParticles length)
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetVelocities")]
        public static extern void GetVelocities(Solver solver, Buffer v, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetVelocities")]
        public static extern void GetVelocities(Solver solver, Buffer v, IntPtr desc = default(IntPtr));

        /**
         * Set the particles phase id array, each particle has an associated phase id which 
         * controls how it interacts with other particles. Particles with phase 0 interact with all
         * other phase types.
         *
         * Particles with a non-zero phase id only interact with particles whose phase differs 
         * from theirs. This is useful, for example, to stop particles belonging to a single
         * rigid shape from interacting with each other.
         * 
         * Phase 0 is used to indicate fluid particles when NvFlexParams::mFluid is set.
         * 
         * @param[in] solver A valid solver
         * @param[in] phases Pointer to a buffer of n integers containing the phases
         * @param[in] desc Describes the copy region, if NULL the solver will try to access the entire buffer (maxParticles length)
         *
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetPhases")]
        public static extern void SetPhases(Solver solver, Buffer phases, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetPhases")]
        public static extern void SetPhases(Solver solver, Buffer phases, IntPtr desc = default(IntPtr));

        /**
         * Get the particle phase ids
         * 
         * @param[in] solver A valid solver
         * @param[out] phases Pointer to a buffer of n integers that will be filled with the phase data, can be either a host or device pointer
         * @param[in] desc Describes the copy region, if NULL the solver will try to access the entire buffer (maxParticles length)
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetPhases")]
        public static extern void GetPhases(Solver solver, Buffer phases, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetPhases")]
        public static extern void GetPhases(Solver solver, Buffer phases, IntPtr desc = default(IntPtr));

        /**
         * Set per-particle normals to the solver, these will be overwritten after each simulation step, but can be used to initialize the normals to valid values
         * 
         * @param[in] solver A valid solver
         * @param[in] normals Pointer to a buffer of normals, should be 4*n in length
         * @param[in] desc Describes the copy region, if NULL the solver will try to access the entire buffer (maxParticles length)
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetNormals")]
        public static extern void SetNormals(Solver solver, Buffer normals, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetNormals")]
        public static extern void SetNormals(Solver solver, Buffer normals, IntPtr desc = default(IntPtr));

        /**
         * Get per-particle normals from the solver, these are the world-space normals computed during surface tension, cloth, and rigid body calculations
         * 
         * @param[in] solver A valid solver
         * @param[out] normals Pointer to a buffer of normals, should be 4*n in length
         * @param[in] desc Describes the copy region, if NULL the solver will try to access the entire buffer (maxParticles length)
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetNormals")]
        public static extern void GetNormals(Solver solver, Buffer normals, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetNormals")]
        public static extern void GetNormals(Solver solver, Buffer normals, IntPtr desc = default(IntPtr));

        /**
         * Set distance constraints for the solver. Each distance constraint consists of two particle indices
         * stored consecutively, a rest-length, and a stiffness value. These are not springs in the traditional
         * sense, but behave somewhat like a traditional spring when lowering the stiffness coefficient.
         * 
         * @param[in] solver A valid solver
         * @param[in] indices Pointer to the spring indices array, should be 2*numSprings length, 2 indices per-spring
         * @param[in] restLengths Pointer to a buffer of rest lengths, should be numSprings length
         * @param[in] stiffness Pointer to the spring stiffness coefficents, should be numSprings in length, a negative stiffness value represents a tether constraint
         * @param[in] numSprings The number of springs to set
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetSprings")]
        public static extern void SetSprings(Solver solver, Buffer indices, Buffer restLengths, Buffer stiffness, int numSprings);

        /**
         * Get the distance constraints from the solver
         * 
         * @param[in] solver A valid solver
         * @param[out] indices Pointer to the spring indices array, should be 2*numSprings length, 2 indices per-spring
         * @param[out] restLengths Pointer to a buffer of rest lengths, should be numSprings length
         * @param[out] stiffness Pointer to the spring stiffness coefficents, should be numSprings in length, a negative stiffness value represents a unilateral tether constraint (only resists stretching, not compression), valid range [-1, 1]
         * @param[in] numSprings The number of springs to get
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetSprings")]
        public static extern void GetSprings(Solver solver, Buffer indices, Buffer restLengths, Buffer stiffness, int numSprings);

        /**
         * Set rigid body constraints for the solver. 
         * @note A particle should not belong to more than one rigid body at a time.
         * 
         * @param[in] solver A valid solver
         * @param[in] offsets Pointer to a buffer of start offsets for a rigid in the indices array, should be numRigids+1 in length, the first entry must be 0
         * @param[in] indices Pointer to a buffer of indices for the rigid bodies, the indices for the jth rigid body start at indices[offsets[j]] and run to indices[offsets[j+1]] exclusive
         * @param[in] restPositions Pointer to a buffer of local space positions relative to the rigid's center of mass (average position), this should be at least 3*numIndices in length in the format x,y,z
         * @param[in] restNormals Pointer to a buffer of local space normals, this should be at least 4*numIndices in length in the format x,y,z,w where w is the (negative) signed distance of the particle inside its shape
         * @param[in] stiffness Pointer to a buffer of rigid stiffness coefficents, should be numRigids in length, valid values in range [0, 1]
         * @param[in] thresholds Pointer to a buffer of plastic deformation threshold coefficients, should be numRigids in length
         * @param[in] creeps Pointer to a buffer of plastic deformation creep coefficients, should be numRigids in length, valid values in range [0, 1]
         * @param[in] rotations Pointer to a buffer of quaternions (4*numRigids in length)
         * @param[in] translations Pointer to a buffer of translations of the center of mass (3*numRigids in length)
         * @param[in] numRigids The number of rigid bodies to set
         * @param[in] numIndices The number of indices in the indices array
         *
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetRigids")]
        public static extern void SetRigids(Solver solver, Buffer offsets, Buffer indices, Buffer restPositions, Buffer restNormals, Buffer stiffness, Buffer thresholds, Buffer creeps, Buffer rotations, Buffer translations, int numRigids, int numIndices);

        /**
         * Retrive the rigid body shape matching constraints and transforms, if any buffer pointers are NULL then they will be ignored
         * This method supersedes the previous NvFlexGetRigidTransforms method and can be used to retrieve modified rest positions from plastic deformation.
         * 
         * @param[in] solver A valid solver
         * @param[in] offsets Pointer to a buffer of start offsets for a rigid in the indices array, should be numRigids+1 in length, the first entry must be 0
         * @param[in] indices Pointer to a buffer of indices for the rigid bodies, the indices for the jth rigid body start at indices[offsets[j]] and run to indices[offsets[j+1]] exclusive
         * @param[in] restPositions Pointer to a buffer of local space positions relative to the rigid's center of mass (average position), this should be at least 3*numIndices in length in the format x,y,z
         * @param[in] restNormals Pointer to a buffer of local space normals, this should be at least 4*numIndices in length in the format x,y,z,w where w is the (negative) signed distance of the particle inside its shape
         * @param[in] stiffness Pointer to a buffer of rigid stiffness coefficents, should be numRigids in length, valid values in range [0, 1]
         * @param[in] thresholds Pointer to a buffer of plastic deformation threshold coefficients, should be numRigids in length
         * @param[in] creeps Pointer to a buffer of plastic deformation creep coefficients, should be numRigids in length, valid values in range [0, 1]
         * @param[in] rotations Pointer to a buffer of quaternions (4*numRigids in length with the imaginary elements in the x,y,z components)
         * @param[in] translations Pointer to a buffer of translations of the center of mass (3*numRigids in length)
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetRigids")]
        public static extern void GetRigids(Solver solver, Buffer offsets, Buffer indices, Buffer restPositions, Buffer restNormals, Buffer stiffness, Buffer thresholds, Buffer creeps, Buffer rotations, Buffer translations);

        /**
         * An opaque type representing a static triangle mesh in the solver
         */
        public struct TriangleMesh { int _; static public implicit operator bool(TriangleMesh _) { return _._ != default(int); } public void Clear() { _ = default(int); } }

        /**
         * An opaque type representing a signed distance field collision shape in the solver
         */
        public struct DistanceField { int _; static public implicit operator bool(DistanceField _) { return _._ != default(int); } public void Clear() { _ = default(int); } }

        /**
         * An opaque type representing a convex mesh collision shape in the solver.
         * Convex mesh shapes may consist of up to 64 planes of the form a*x + b*y + c*z + d = 0,
         * particles will be constrained to the outside of the shape.
         */
        public struct ConvexMesh { int _; static public implicit operator bool(ConvexMesh _) { return _._ != default(int); } public void Clear() { _ = default(int); } }

        /**
         * Create triangle mesh geometry, note that meshes may be used by multiple solvers if desired
         * 
         * @param[in] lib The library instance to use
         * @return ID of a triangle mesh object
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexCreateTriangleMesh")]
        public static extern TriangleMesh CreateTriangleMesh(Library lib);

        /**
         * Destroy a triangle mesh created with NvFlexCreateTriangleMesh()
         *
         * @param[in] lib The library instance to use
         * @param[in] mesh A triangle mesh created with NvFlexCreateTriangleMesh()
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexDestroyTriangleMesh")]
        public static extern void DestroyTriangleMesh(Library lib, TriangleMesh mesh);

        /**
         * Specifies the triangle mesh geometry (vertices and indices), this method will cause any internal
         * data structures (e.g.: bounding volume hierarchies) to be rebuilt.
         * 
         * @param[in] lib The library instance to use
         * @param[in] mesh A triangle mesh created with NvFlexCreateTriangleMesh()
         * @param[in] vertices Pointer to a buffer of float3 vertex positions
         * @param[in] indices Pointer to a buffer of triangle indices, should be length numTriangles*3
         * @param[in] numVertices The number of vertices in the vertices array
         * @param[in] numTriangles The number of triangles in the mesh
         * @param[in] lower A pointer to a float3 vector holding the lower spatial bounds of the mesh
         * @param[in] upper A pointer to a float3 vector holding the upper spatial bounds of the mesh
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexUpdateTriangleMesh")]
        public static extern void UpdateTriangleMesh(Library lib, TriangleMesh mesh, Buffer vertices, Buffer indices, int numVertices, int numTriangles, ref Vector3 lower, ref Vector3 upper);

        /**
         * Retrieve the local space bounds of the mesh, these are the same values specified to NvFlexUpdateTriangleMesh()
         * 
         * @param[in] lib The library instance to use
         * @param[in] mesh Pointer to a triangle mesh object
         * @param[out] lower Pointer to a buffer of 3 floats that the lower mesh bounds will be written to
         * @param[out] upper Pointer to a buffer of 3 floats that the upper mesh bounds will be written to
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetTriangleMeshBounds")]
        public static extern void GetTriangleMeshBounds(Library lib, TriangleMesh mesh, ref Vector3 lower, ref Vector3 upper);

        /**
         * Create a signed distance field collision shape, see NvFlexDistanceFieldId for details.
         * 
         * @param[in] lib The library instance to use
         * @return A pointer to a signed distance field object
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexCreateDistanceField")]
        public static extern DistanceField CreateDistanceField(Library lib);

        /**
         * Destroy a signed distance field
         * 
         * @param[in] lib The library instance to use
         * @param[in] sdf A signed distance field created with NvFlexCreateDistanceField()
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexDestroyDistanceField")]
        public static extern void DestroyDistanceField(Library lib, DistanceField sdf);

        /**
         * Update the signed distance field volume data, this method will upload
         * the field data to a 3D texture on the GPU
         * 
         * @param[in] lib The library instance to use
         * @param[in] sdf A signed distance field created with NvFlexCreateDistanceField()
         * @param[in] dimx The x-dimension of the volume data in voxels
         * @param[in] dimy The y-dimension of the volume data in voxels
         * @param[in] dimz The z-dimension of the volume data in voxels
         * @param[in] field The volume data stored such that the voxel at the x,y,z coordinate is addressed as field[z*dimx*dimy + y*dimx + x]
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexUpdateDistanceField")]
        public static extern void UpdateDistanceField(Library lib, DistanceField sdf, int dimx, int dimy, int dimz, Buffer field);

        /**
         * Create a convex mesh collision shapes, see NvFlexConvexMeshId for details.
         * 
         * @param[in] lib The library instance to use
         * @return A pointer to a signed distance field object
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexCreateConvexMesh")]
        public static extern ConvexMesh CreateConvexMesh(Library lib);

        /**
         * Destroy a convex mesh
         * 
         * @param[in] lib The library instance to use
         * @param[in] convex A a convex mesh created with NvFlexCreateConvexMesh()
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexDestroyConvexMesh")]
        public static extern void DestroyConvexMesh(Library lib, ConvexMesh convex);

        /**
         * Update the convex mesh geometry
         * 
         * @param[in] lib The library instance to use
         * @param[in] convex A valid convex mesh shape created from NvFlexCreateConvexMesh()
         * @param[in] planes An array of planes, each plane consists of 4 floats in the form a*x + b*y + c*z + d = 0
         * @param[in] numPlanes The number of planes in the convex
         * @param[in] lower The local space lower bound of the convex shape
         * @param[in] upper The local space upper bound of the convex shape
          */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexUpdateConvexMesh")]
        public static extern void UpdateConvexMesh(Library lib, ConvexMesh convex, Buffer planes, int numPlanes, ref Vector3 lower, ref Vector3 upper);

        /**
         * Retrieve the local space bounds of the mesh, these are the same values specified to NvFlexUpdateConvexMesh()
         * 
         * @param[in] lib The library instance to use
         * @param[in] mesh Pointer to a convex mesh object
         * @param[out] lower Pointer to a buffer of 3 floats that the lower mesh bounds will be written to
         * @param[out] upper Pointer to a buffer of 3 floats that the upper mesh bounds will be written to
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetConvexMeshBounds")]
        public static extern void GetConvexMeshBounds(Library lib, ConvexMesh mesh, ref Vector3 lower, ref Vector3 upper);

        /**
         * A basic sphere shape with origin at the center of the sphere and radius
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct SphereGeometry
        {
            public float radius;
        };

        /**
         * A collision capsule extends along the x-axis with it's local origin at the center of the capsule 
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct CapsuleGeometry
        {
            public float radius;
            public float halfHeight;
        };

        /**
         * A simple box with interior [-halfHeight, +halfHeight] along each dimension 
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct BoxGeometry
        {
            public Vector3 halfExtents;
        };

        /**
         * A convex mesh instance with non-uniform scale
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct ConvexMeshGeometry
        {
            public Vector3 scale;
            public ConvexMesh mesh;
        };

        /**
         * A scaled triangle mesh
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct TriangleMeshGeometry
        {
            public Vector3 scale;       //!< The scale of the object from local space to world space
            public TriangleMesh mesh;   //!< A triangle mesh pointer created by flexCreateTriangleMesh()
        };

        /**
         * A scaled signed distance field
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct SDFGeometry
        {
            public float scale;         //!< Uniform scale of SDF, this corresponds to the world space width of the shape
            public DistanceField field; //!< A signed distance field pointer created by NvFlexCreateDistanceField()
        };

        /**
         * This union allows collision geometry to be sent to Flex as a flat array of 16-byte data structures,
         * the shape flags array specifies the type for each shape, see flexSetShapes().
         */
        [StructLayout(LayoutKind.Explicit)]
        public struct CollisionGeometry
        {
            [FieldOffset(0)] public SphereGeometry sphere;
            [FieldOffset(0)] public CapsuleGeometry capsule;
            [FieldOffset(0)] public BoxGeometry box;
            [FieldOffset(0)] public ConvexMeshGeometry convexMesh;
            [FieldOffset(0)] public TriangleMeshGeometry triMesh;
            [FieldOffset(0)] public SDFGeometry sdf;
        };

        public enum CollisionShapeType
        {
            Sphere = 0,         //!< A sphere shape, see FlexSphereGeometry
            Capsule = 1,        //!< A capsule shape, see FlexCapsuleGeometry
            Box = 2,            //!< A box shape, see FlexBoxGeometry
            ConvexMesh = 3,     //!< A convex mesh shape, see FlexConvexMeshGeometry
            TriangleMesh = 4,   //!< A triangle mesh shape, see FlexTriangleMeshGeometry
            SDF = 5,            //!< A signed distance field shape, see FlexSDFGeometry
        };

        public enum CollisionShapeFlags
        {
            TypeMask = 0x7,     //!< Lower 3 bits holds the type of the collision shape
            Dynamic = 8,        //!< Indicates the shape is dynamic and should have lower priority over static collision shapes
            Trigger = 16,       //!< Indicates that the shape is a trigger volume, this means it will not perform any collision response, but will be reported in the contacts array (see NvFlexGetContacts())

            Reserved = -256     // 0xffffff00
        };

        /** 
         * Helper function to combine shape type, flags, and phase/shape collision channels into a 32bit value
         * 
         * @param[in] type The type of the shape, see NvFlexCollisionShapeType
         * @param[in] dynamic See eNvFlexShapeFlagDynamic
         * @param[in] shapeChannels A combination of the eNvFlexPhaseShapeChannel* flags, collisions will only be processed between a particle and a shape if a channel is set on both the particle and shape, see NvFlexMakePhaseWithChannels()
         */
        public static int MakeShapeFlagsWithChannels(CollisionShapeType type, bool dynamic, int shapeChannels) { return (int)type | (dynamic ? (int)CollisionShapeFlags.Dynamic : 0) | shapeChannels; }

        /** 
         * Deprecrated helper method that creates shape flags that by default have all collision channels enabled
         */
        public static int MakeShapeFlags(CollisionShapeType type, bool dynamic) { return MakeShapeFlagsWithChannels(type, dynamic, (int)Phase.ShapeChannelMask); }

        /**
         * Set the collision shapes for the solver
         * 
         * @param[in] solver A valid solver
         * @param[in] geometry Pointer to a buffer of NvFlexCollisionGeometry entries, the type of each shape determines how many entries it has in the array
         * @param[in] shapePositions Pointer to a buffer of translations for each shape in world space, should be 4*numShapes in length
         * @param[in] shapeRotations Pointer to an a buffer of rotations for each shape stored as quaternion, should be 4*numShapes in length
         * @param[in] shapePrevPositions Pointer to a buffer of translations for each shape at the start of the time step, should be 4*numShapes in length
         * @param[in] shapePrevRotations Pointer to an a buffer of rotations for each shape stored as a quaternion at the start of the time step, should be 4*numShapees in length
         * @param[in] shapeFlags The type and behavior of the shape, NvFlexCollisionShapeFlags for more detail
         * @param[in] numShapes The number of shapes
         *
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetShapes")]
        public static extern void SetShapes(Solver solver, Buffer geometry, Buffer shapePositions, Buffer shapeRotations, Buffer shapePrevPositions, Buffer shapePrevRotations, Buffer shapeFlags, int numShapes);

        /**
         * Set dynamic triangles mesh indices, typically used for cloth. Flex will calculate normals and 
         * apply wind and drag effects to connected particles. See NvFlexParams::drag, NvFlexParams::wind.
         * 
         * @param[in] solver A valid solver
         * @param[in] indices Pointer to a buffer of triangle indices into the particles array, should be 3*numTris in length
         * @param[in] normals Pointer to a buffer of triangle normals, should be 3*numTris in length, can be NULL
         * @param[in] numTris The number of dynamic triangles
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetDynamicTriangles")]
        public static extern void SetDynamicTriangles(Solver solver, Buffer indices, Buffer normals, int numTris);

        /**
         * Get the dynamic triangle indices and normals.
         * 
         * @param[in] solver A valid solver
         * @param[out] indices Pointer to a buffer of triangle indices into the particles array, should be 3*numTris in length, if NULL indices will not be returned
         * @param[out] normals Pointer to a buffer of triangle normals, should be 3*numTris in length, if NULL normals will be not be returned
         * @param[in] numTris The number of dynamic triangles
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetDynamicTriangles")]
        public static extern void GetDynamicTriangles(Solver solver, Buffer indices, Buffer normals, int numTris);

        /**
         * Set inflatable shapes, an inflatable is a range of dynamic triangles (wound CCW) that represent a closed mesh.
         * Each inflatable has a given rest volume, constraint scale (roughly equivalent to stiffness), and "over pressure"
         * that controls how much the shape is inflated.
         * 
         * @param[in] solver A valid solver
         * @param[in] startTris Pointer to a buffer of offsets into the solver's dynamic triangles for each inflatable, should be numInflatables in length
         * @param[in] numTris Pointer to a buffer of triangle counts for each inflatable, should be numInflatablesin length
         * @param[in] restVolumes Pointer to a buffer of rest volumes for the inflatables, should be numInflatables in length
         * @param[in] overPressures Pointer to a buffer of floats specifying the pressures for each inflatable, a value of 1.0 means the rest volume, > 1.0 means over-inflated, and < 1.0 means under-inflated, should be numInflatables in length
         * @param[in] constraintScales Pointer to a buffer of scaling factors for the constraint, this is roughly equivalent to stiffness but includes a constraint scaling factor from position-based dynamics, see helper code for details, should be numInflatables in length
         * @param[in] numInflatables Number of inflatables to set
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetInflatables")]
        public static extern void SetInflatables(Solver solver, Buffer startTris, Buffer numTris, Buffer restVolumes, Buffer overPressures, Buffer constraintScales, int numInflatables);

        /**
         * Get the density values for fluid particles
         *
         * @param[in] solver A valid solver
         * @param[out] densities Pointer to a buffer of floats, should be maxParticles in length, density values are normalized between [0, 1] where 1 represents the rest density
         * @param[in] desc Pointer to a descriptor specifying the contents to read back
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetDensities")]
        public static extern void GetDensities(Solver solver, Buffer densities, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetDensities")]
        public static extern void GetDensities(Solver solver, Buffer densities, IntPtr desc = default(IntPtr));

        /**
         * Get the anisotropy of fluid particles, the particle distribution for a particle is represented
         * by 3 orthogonal vectors. Each 3-vector has unit length with the variance along that axis
         * packed into the w component, i.e.: x,y,z,lambda.
        *
         * The anisotropy defines an oriented ellipsoid in worldspace that can be used for rendering
         * or surface extraction.
         *
         * @param[in] solver A valid solver
         * @param[out] q1 Pointer to a buffer of floats that receive the first basis vector and scale, should be 4*maxParticles in length
         * @param[out] q2 Pointer to a buffer of floats that receive the second basis vector and scale, should be 4*maxParticles in length
         * @param[out] q3 Pointer to a buffer of floats that receive the third basis vector and scale, should be 4*maxParticles in length
         * @param[in] desc Pointer to a descriptor specifying the contents to read back
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetAnisotropy")]
        public static extern void GetAnisotropy(Solver solver, Buffer q1, Buffer q2, Buffer q3, ref CopyDesc desc);
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetAnisotropy")]
        public static extern void GetAnisotropy(Solver solver, Buffer q1, Buffer q2, Buffer q3, IntPtr desc = default(IntPtr));

        /**
         * Get the state of the diffuse particles. Diffuse particles are passively advected by the fluid
         * velocity field.
         *
         * @param[in] solver A valid solver
         * @param[out] p Pointer to a buffer of floats, should be 4*maxParticles in length, the w component represents the particles lifetime with 1 representing a new particle, and 0 representing an inactive particle
         * @param[out] v Pointer to a buffer of floats, should be 4*maxParticles in length, the w component is not used
         * @param[out] count Pointer to a buffer of a single int that holds the current particle count (this may be updated by the GPU which is why it is passed back in a buffer)
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetDiffuseParticles")]
        public static extern void GetDiffuseParticles(Solver solver, Buffer p, Buffer v, Buffer count);

        /**
         * Set the state of the diffuse particles. Diffuse particles are passively advected by the fluid
         * velocity field.
         *
         * @param[in] solver A valid solver
         * @param[in] p Pointer to a buffer of floats, should be 4*n in length, the w component represents the particles lifetime with 1 representing a new particle, and 0 representing an inactive particle
         * @param[in] v Pointer to a buffer of floats, should be 4*n in length, the w component is not used
         * @param[in] n Number of diffuse particles to set
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexSetDiffuseParticles")]
        public static extern void SetDiffuseParticles(Solver solver, Buffer p, Buffer v, int n);

        /**
         * Get the particle contact planes. Note this will only include contacts that were active on the last substep of an update, and will include all contact planes generated within NvFlexParams::shapeCollisionMargin.
         *
         * @param[in] solver A valid solver
         * @param[out] planes Pointer to a destination buffer containing the contact planes for the particle, each particle can have up to 4 contact planes so this buffer should be 16*maxParticles in length
         * @param[out] velocities Pointer to a destination buffer containing the velocity of the contact point on the shape in world space, the index of the shape (corresponding to the shape in NvFlexSetShapes() is stored in the w component), each particle can have up to 4 contact planes so this buffer should be 16*maxParticles in length
         * @param[out] indices Pointer to a buffer of indices into the contacts buffer, the first contact plane for the i'th particle is given by planes[indices[i]*sizeof(float)*4] and subsequent contacts for that particle are stored sequentially, this array should be maxParticles in length
         * @param[out] counts Pointer to a buffer of contact counts for each particle (will be <= 4), this buffer should be maxParticles in length
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetContacts")]
        public static extern void GetContacts(Solver solver, Buffer planes, Buffer velocities, Buffer indices, Buffer counts);

        /**
         * Get the particle neighbor lists, these are stored in a strided format, and can be iterated in the following manner:
         *
\code{.c}

            NvFlexGetNeighbors(solver, neighborsBuffer, countsBuffer, indicesBuffer);

            int* neighbors = (int*)NvFlexMap(neighborsBuffer, 0);
            int* counts = (int*)NvFlexMap(countsBuffer, 0);
            int* remap = (int*)NvFlexMap(remapBuffer, 0);

            // neighbors are stored in a strided format so that the first neighbor
            // of each particle is stored sequentially, then the second, and so on
            
            int stride = maxParticles;

            for (int i=0; i < maxParticles; ++i)
            {
                // find offset in the neighbors buffer
                int offset = remap[i];
                int count = counts[offset];

                for (int c=0; c < count; ++c)
                {
                    int neighbor = remap[neighbors[c*stride + offset]];

                    printf("Particle %d's neighbor %d is particle %d\n", i, c, neighbor);
                }
            }

            NvFlexUnmap(neighborsBuffer);
            NvFlexUnmap(countsBuffer);
            NvFlexUnmap(remapBuffer);

\endcode
         *
         * @param[in] solver A valid solver
         * @param[out] neighbors Pointer to a destination buffer containing the the neighbors for all particles, this should be maxParticles*maxParticleNeighbors ints (passed to NvFlexInit() in length)
         * @param[out] counts Pointer to a buffer of neighbor counts per-particle, should be maxParticles ints in length
         * @param[out] remap Pointer to a buffer of indices, because Flex internally re-orders particles these are used to map from an API particle index to it internal index
         *
         * @note Neighbors are only valid after a call to NvFlexUpdateSolver() has completed, the returned neighbors correspond to the last substep of the last update
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetNeighbors")]
        public static extern void GetNeighbors(Solver solver, Buffer neighbors, Buffer counts, Buffer remap);

        /**
         * Get the world space AABB of all particles in the solver, note that the bounds are calculated during the update (see NvFlexUpdateSolver()) so only become valid after an update has been performed.
         * The returned bounds represent bounds of the particles in their predicted positions *before* the constraint solve.
         * 
         * @param[in] solver A valid solver
         * @param[out] lower Pointer to a buffer of 3 floats to receive the lower bounds
         * @param[out] upper Pointer to a buffer of 3 floats to receive the upper bounds
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetBounds")]
        public static extern void GetBounds(Solver solver, Buffer lower, Buffer upper);

        /**
         *
         * @param[in] solver A valid solver
         * @param[out] begin Optional pointer to a 64 bit unsigned to receive the value of the GPU clock when Flex update began (in cycles)
         * @param[out] end Optional pointer to a 64 bit unsigned to receive the value of the GPU clock when Flex update ended (in cycles)
         * @param[out] frequency Optional pointer to a 64 bit unsigned to receive the frequency of the clock used to measure begin and end
         * @return The time in seconds between the first and last GPU operations executed by the last NvFlexUpdateSolver.
         *
         * @note This method causes the CPU to wait until the GPU has finished any outstanding work. 
         *		 To avoid blocking the calling thread it should be called after work has completed, e.g.: directly after a NvFlexMap().
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetDeviceLatency")]
        public static extern float GetDeviceLatency(Solver solver, IntPtr begin, IntPtr end, IntPtr frequency);

        /**
         * Fetch high-level GPU timers.
         *
         * @param[in] solver The solver instance to use
         * @param[out] timers A struct containing the GPU latency of each stage in the physics pipeline.
         *
         * @note This method causes the CPU to wait until the GPU has finished any outstanding work.
         *		 To avoid blocking the calling thread it should be called after work has completed, e.g.: directly after a NvFlexMap().
         *       To capture there timers you must pass true for enableTimers in NvFlexUpdateSolver()
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetTimers")]
        public static extern void GetTimers(Solver solver, ref Timers timers);

        /**
        * Holds the execution time for a specfic shader
        */
        [StructLayout(LayoutKind.Sequential)]
        public struct DetailTimer
        {
            public IntPtr name;
            public float time;
        };

        /**
         * Fetch per-shader GPU timers.
         *
         * @param[in] solver The solver instance to use
         * @param[out] timers An array of NvFlexDetailTimer structures, each representing a unique shader.
         * @return The number of detail timers in the timers array
         *
         * @note This method causes the CPU to wait until the GPU has finished any outstanding work.
         *		To avoid blocking the calling thread it should be called after work has completed, e.g.: directly after a NvFlexMap().
         *       To capture there timers you must pass true for enableTimers in NvFlexUpdateSolver()
         *		Timers are valid until the next call to NvFlexGetDetailTimers
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetDetailTimers")]
        public static extern int GetDetailTimers(Solver solver, ref IntPtr timers);

        /**
         * Allocate a Flex buffer. Buffers are used to pass data to the API in an efficient manner.
         *
         * @param[in] lib The library instance to use
         * @param[in] elementCount The number of elements in the buffer
         * @param[in] elementByteStride The size of each element in bytes
         * @param[in] type The type of buffer to allocate, can be either host memory or device memory
         * @return A pointer to a NvFlexBuffer
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexAllocBuffer")]
        public static extern Buffer AllocBuffer(Library lib, int elementCount, int elementByteStride, BufferType type);

        /**
         * Free a Flex buffer
         *
         * @param[in] buf A buffer to free, must be allocated with NvFlexAllocBuffer()
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexFreeBuffer")]
        public static extern void FreeBuffer(Buffer buf);

        /**
         * Maps a buffer for reading and writing. When the buffer is created with NvFlexBufferType::eHost, then the returned pointer will be a host memory address
         * that can be read/written.
         * Mapping a buffer implicitly synchronizes with the GPU to ensure that any reads or writes from the buffer (e.g.: from the NvFlexGet*() or NvFlexSet*()) methods have completed.
         *
         * @param[in] buffer A buffer allocated with NvFlexAllocBuffer()
         * @param[in] flags Hints to Flex how the buffer is to be accessed, typically this should be eNvFlexMapWait (0)
         * @return A pointer to the mapped memory
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexMap")]
        public static extern IntPtr Map(Buffer buffer, MapFlags flags = MapFlags.Wait);

        /**
         * Unmaps a buffer that was mapped through NvFlexMap(), note that buffers must be unmapped before they can be passed to a NvFlexGet*() or NvFlexSet*() method
         *
         * @param[in] buffer A valid buffer allocated through NvFlexAllocBuffer()
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexUnmap")]
        public static extern void Unmap(Buffer buffer);



















        /** 
         * Returns a null-terminated string with the compute device name
         *
         * @param[in] lib The library instance to use
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexGetDeviceName")]
        public static extern IntPtr GetDeviceName(Library lib);

        /**
         * Force a pipeline flush to ensure any queued work is submitted to the GPU
         *
         * @param[in] lib The library instance to use
         */
        [DllImport(FLEX_DLL, EntryPoint = "NvFlexFlush")]
        public static extern void Flush(Library lib);
    }
}
