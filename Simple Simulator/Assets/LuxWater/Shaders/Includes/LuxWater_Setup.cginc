
//  Define Fog Mode
//	Make sure only one fog mode is defined.

//  Built in fog modes ------------------
//  #define FOG_LINEAR
//  #define FOG_EXP
    #define FOG_EXP2

//  Azure Fog ---------------------------
//  #define FOG_AZUR
//	#include "Assets/Azure[Sky] Dynamic Skybox/Shaders/Transparent/AzureFogCore.cginc"
        
//  Enviro Fog --------------------------
//  #define FOG_ENVIRO
//	#include "Assets/Enviro - Sky and Weather/Core/Resources/Shaders/Core/EnviroFogCore.cginc"


//  Other features --------------------------

//	Uncomment to make the shader use disney diffuse lighting on foam. Otherwise it uses simple NdotL
//	#define DISNEYDIFFUSE