// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "KumaBeer/Anime_water"
{
	Properties
	{
		_Maincolor("Main color", Color) = (0.2117647,0.5529412,0.5843138,1)
		_TessValue( "Max Tessellation", Range( 1, 32 ) ) = 32
		_TessMin( "Tess Min Distance", Float ) = 0
		_TessMax( "Tess Max Distance", Float ) = 35
		_Wavestime("Waves time", Range( 0 , 1)) = 1
		_Wavestile("Waves tile", Float) = 1
		_WavesNormal("Waves Normal", 2D) = "bump" {}
		_WavesNormalscale("Waves Normal scale", Range( 0.1 , 1)) = 0.3
		_Distortion("Distortion", Float) = 0.5
		_DepthColor("Depth Color", Color) = (0,0.04313726,0.4039216,1)
		_WaterDepth("Water Depth", Float) = 0
		_WaterFalloff("Water Falloff", Float) = -3.5
		_Specular("Specular", Range( -1 , 0)) = -0.624
		[HDR]_RimColor("Rim Color", Color) = (0.1333333,0.3058824,0.3098039,0)
		_RimOffset("Rim Offset", Range( 0 , 1)) = 0.564
		_RimStr("Rim Str", Range( 0.01 , 1)) = 0.95
		_Sparklestile("Sparkles tile", Float) = 25
		_CausticSparklesStr("Caustic/Sparkles Str", Float) = 1
		_Caustictile("Caustic tile", Float) = 15
		_Causticalignmin("Caustic align min", Float) = -3
		_Causticalignmax("Caustic align max", Float) = 1
		_Foamtile("Foam tile", Float) = 10
		_Foamcolor("Foam color", Color) = (0.7372549,0.7372549,0.7372549,0)
		_FoamFalloff("Foam Falloff", Float) = 1.15
		_FoamDepth("Foam Depth", Float) = 1.5
		_FoamIntensity("Foam Intensity", Range( 0 , 5)) = 0.5
		_WaterMask("Water Mask", 2D) = "white" {}
		_Surf("Surf", Float) = 2
		_Surfclamp("Surf clamp", Range( 0 , 1)) = 0.05
		_TideebbStr("Tide-ebb Str", Float) = 0.11
		[Toggle(_USEREFLECTION_ON)] _Usereflection("Use reflection", Float) = 1
		_Reflectstr("Reflect str", Range( 0 , 1)) = 0.4
		_Reflectpos("Reflect pos", Float) = 0
		_ReflectionTex("_ReflectionTex", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		GrabPass{ "_GrabScreen0" }
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Tessellation.cginc"
		#pragma target 4.6
		#pragma shader_feature _USEREFLECTION_ON
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf StandardCustomLighting keepalpha noshadow vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			half3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
			float4 screenPos;
			float3 worldPos;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform sampler2D _WavesNormal;
		uniform half _Wavestime;
		uniform half _Wavestile;
		uniform half _WavesNormalscale;
		uniform half _Surfclamp;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform half _TideebbStr;
		uniform float _RimStr;
		uniform float _RimOffset;
		uniform float4 _RimColor;
		uniform half4 _Maincolor;
		uniform sampler2D _ReflectionTex;
		uniform half _Reflectpos;
		uniform float _Distortion;
		uniform half _Reflectstr;
		uniform float _FoamDepth;
		uniform sampler2D _WaterMask;
		uniform half _Foamtile;
		uniform float _FoamFalloff;
		uniform half _FoamIntensity;
		uniform half _Surf;
		uniform half4 _Foamcolor;
		uniform half _Caustictile;
		uniform half _Causticalignmin;
		uniform half _Causticalignmax;
		uniform half _CausticSparklesStr;
		uniform half _Specular;
		uniform half _Sparklestile;
		uniform float4 _DepthColor;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabScreen0 )
		uniform float _WaterDepth;
		uniform float _WaterFalloff;
		uniform float _TessValue;
		uniform float _TessMin;
		uniform float _TessMax;


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityDistanceBasedTess( v0.vertex, v1.vertex, v2.vertex, _TessMin, _TessMax, _TessValue );
		}

		void vertexDataFunc( inout appdata_full v )
		{
			half mulTime896 = _Time.y * _Wavestime;
			half2 temp_cast_0 = (_Wavestile).xx;
			float2 uv_TexCoord477 = v.texcoord.xy * temp_cast_0;
			half2 panner475 = ( mulTime896 * float2( 0.04,0.03 ) + uv_TexCoord477);
			float4 ase_screenPos = ComputeScreenPos( UnityObjectToClipPos( v.vertex ) );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			half eyeDepth1 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_LOD( _CameraDepthTexture, float4( ase_screenPosNorm.xy, 0, 0 ) ));
			half temp_output_89_0 = abs( ( eyeDepth1 - ase_screenPos.w ) );
			half temp_output_794_0 = ( 1.0 - temp_output_89_0 );
			half lerpResult814 = lerp( _WavesNormalscale , _Surfclamp , saturate( temp_output_794_0 ));
			half2 panner829 = ( mulTime896 * float2( -0.03,0.04 ) + uv_TexCoord477);
			half3 temp_output_826_0 = BlendNormals( UnpackScaleNormal( tex2Dlod( _WavesNormal, float4( panner475, 0, 1.0) ), lerpResult814 ) , UnpackScaleNormal( tex2Dlod( _WavesNormal, float4( panner829, 0, 1.0) ), lerpResult814 ) );
			half temp_output_365_0 = sin( _Time.y );
			half3 ase_vertexNormal = v.normal.xyz;
			half3 temp_cast_1 = (( temp_output_826_0.y + ( temp_output_365_0 * ase_vertexNormal.y * _TideebbStr ) )).xxx;
			v.vertex.xyz += temp_cast_1;
			v.vertex.w = 1;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			half4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			half mulTime896 = _Time.y * _Wavestime;
			half2 temp_cast_9 = (_Wavestile).xx;
			float2 uv_TexCoord477 = i.uv_texcoord * temp_cast_9;
			half2 panner475 = ( mulTime896 * float2( 0.04,0.03 ) + uv_TexCoord477);
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			half eyeDepth1 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			half temp_output_89_0 = abs( ( eyeDepth1 - ase_screenPos.w ) );
			half temp_output_794_0 = ( 1.0 - temp_output_89_0 );
			half lerpResult814 = lerp( _WavesNormalscale , _Surfclamp , saturate( temp_output_794_0 ));
			half2 panner829 = ( mulTime896 * float2( -0.03,0.04 ) + uv_TexCoord477);
			half3 temp_output_826_0 = BlendNormals( UnpackScaleNormal( tex2D( _WavesNormal, panner475 ), lerpResult814 ) , UnpackScaleNormal( tex2D( _WavesNormal, panner829 ), lerpResult814 ) );
			half4 screenColor358 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabScreen0,( half3( (ase_grabScreenPosNorm).xy ,  0.0 ) + ( temp_output_826_0 * _Distortion ) ).xy);
			half4 lerpResult93 = lerp( _DepthColor , screenColor358 , saturate( pow( ( temp_output_89_0 + _WaterDepth ) , _WaterFalloff ) ));
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			half4 ase_lightColor = 0;
			#else //aselc
			half4 ase_lightColor = _LightColor0;
			#endif //aselc
			half4 Lightcolor233 = ase_lightColor;
			c.rgb = ( lerpResult93 * ase_lightAtten * Lightcolor233 ).rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
			half mulTime896 = _Time.y * _Wavestime;
			half2 temp_cast_0 = (_Wavestile).xx;
			float2 uv_TexCoord477 = i.uv_texcoord * temp_cast_0;
			half2 panner475 = ( mulTime896 * float2( 0.04,0.03 ) + uv_TexCoord477);
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			half eyeDepth1 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			half temp_output_89_0 = abs( ( eyeDepth1 - ase_screenPos.w ) );
			half temp_output_794_0 = ( 1.0 - temp_output_89_0 );
			half lerpResult814 = lerp( _WavesNormalscale , _Surfclamp , saturate( temp_output_794_0 ));
			half2 panner829 = ( mulTime896 * float2( -0.03,0.04 ) + uv_TexCoord477);
			half3 temp_output_826_0 = BlendNormals( UnpackScaleNormal( tex2D( _WavesNormal, panner475 ), lerpResult814 ) , UnpackScaleNormal( tex2D( _WavesNormal, panner829 ), lerpResult814 ) );
			float3 newWorldNormal261 = (WorldNormalVector( i , temp_output_826_0 ));
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			half3 ase_worldlightDir = 0;
			#else //aseld
			half3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			half dotResult240 = dot( newWorldNormal261 , ase_worldlightDir );
			half3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			half dotResult242 = dot( newWorldNormal261 , ase_worldViewDir );
			half temp_output_244_0 = ( dotResult242 + _RimOffset );
			half smoothstepResult265 = smoothstep( _RimStr , _RimOffset , temp_output_244_0);
			half temp_output_255_0 = saturate( ( dotResult240 * smoothstepResult265 ) );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			half4 ase_lightColor = 0;
			#else //aselc
			half4 ase_lightColor = _LightColor0;
			#endif //aselc
			half4 Lightcolor233 = ase_lightColor;
			half4 temp_cast_2 = (0.0).xxxx;
			half2 appendResult770 = (half2(ase_screenPosNorm.x , ase_screenPosNorm.y));
			half3 ase_worldNormal = WorldNormalVector( i, half3( 0, 0, 1 ) );
			half3 ase_worldTangent = WorldNormalVector( i, half3( 1, 0, 0 ) );
			half3 ase_worldBitangent = WorldNormalVector( i, half3( 0, 1, 0 ) );
			half3x3 ase_tangentToWorldFast = float3x3(ase_worldTangent.x,ase_worldBitangent.x,ase_worldNormal.x,ase_worldTangent.y,ase_worldBitangent.y,ase_worldNormal.y,ase_worldTangent.z,ase_worldBitangent.z,ase_worldNormal.z);
			half3 tangentToViewDir216 = mul( UNITY_MATRIX_V, float4( mul( ase_tangentToWorldFast, temp_output_826_0 ), 0 ) ).xyz;
			half3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			half4 tex2DNode227 = tex2D( _ReflectionTex, ( appendResult770 + ( (( tangentToViewDir216 - ase_vertexNormal )).xy * _Reflectpos ) + ( (temp_output_826_0).xy * _Distortion ) ) );
			half4 lerpResult210 = lerp( _Maincolor , tex2DNode227 , ( temp_output_244_0 * _Reflectstr * saturate( temp_output_89_0 ) ));
			#ifdef _USEREFLECTION_ON
				half4 staticSwitch900 = lerpResult210;
			#else
				half4 staticSwitch900 = temp_cast_2;
			#endif
			half temp_output_365_0 = sin( _Time.y );
			half temp_output_364_0 = ( temp_output_365_0 * ( 1.0 - dotResult242 ) );
			half2 temp_cast_3 = (_Foamtile).xx;
			float2 uv_TexCoord106 = i.uv_texcoord * temp_cast_3;
			half2 panner116 = ( temp_output_364_0 * float2( 0.5,0.5 ) + uv_TexCoord106);
			half4 tex2DNode668 = tex2D( _WaterMask, panner116 );
			half smoothstepResult335 = smoothstep( 1.2 , 1.0 , ( ( temp_output_89_0 * _FoamDepth * tex2DNode668.b ) + _FoamFalloff ));
			half lerpResult437 = lerp( 200.0 , 0.0 , temp_output_794_0);
			half temp_output_441_0 = (lerpResult437*_FoamIntensity + 0.0);
			half temp_output_688_0 = sin( ( ( (1.0 + (temp_output_89_0 - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) - ( _Time.y * 0.4 ) ) * 12.0 ) );
			half lerpResult712 = lerp( temp_output_688_0 , 1.0 , _Surf);
			half Foam338 = saturate( ( smoothstepResult335 * temp_output_441_0 * lerpResult712 ) );
			half2 temp_cast_4 = (_Caustictile).xx;
			float2 uv_TexCoord737 = i.uv_texcoord * temp_cast_4;
			half clampResult893 = clamp( temp_output_794_0 , _Causticalignmin , _Causticalignmax );
			half2 panner22 = ( 1.0 * _Time.y * float2( -0.03,0 ) + ( uv_TexCoord737 + clampResult893 ));
			half4 tex2DNode105 = tex2D( _WaterMask, panner22 );
			half clampResult853 = clamp( temp_output_794_0 , 0.05 , 1.0 );
			half clampResult724 = clamp( ( tex2DNode105.r * _CausticSparklesStr * clampResult853 * saturate( temp_output_441_0 ) * ase_lightColor.a ) , 0.0 , 1.0 );
			half clampResult797 = clamp( temp_output_364_0 , 0.0 , 4.0 );
			half smoothstepResult800 = smoothstep( -2.0 , 8.0 , temp_output_688_0);
			half4 lerpResult841 = lerp( Lightcolor233 , tex2DNode227 , 0.6);
			half lerpResult758 = lerp( _Specular , temp_output_255_0 , 0.45);
			half clampResult750 = clamp( sign( lerpResult758 ) , 0.0 , 0.8 );
			half2 temp_cast_5 = (_Sparklestile).xx;
			float2 uv_TexCoord860 = i.uv_texcoord * temp_cast_5 + ( ase_screenPos * dotResult242 ).xy;
			half4 lerpResult836 = lerp( float4( 0,0,0,0 ) , lerpResult841 , ( clampResult750 + saturate( (( UnpackNormal( tex2D( _WaterMask, uv_TexCoord860 ) ).g * tex2DNode668.b * tex2DNode105.r * _CausticSparklesStr )*20.0 + -0.5) ) ));
			o.Emission = ( ( temp_output_255_0 * half4( (_RimColor).rgb , 0.0 ) * Lightcolor233 ) + staticSwitch900 + ( Foam338 * _Foamcolor ) + clampResult724 + saturate( ( temp_output_794_0 * clampResult797 * smoothstepResult800 ) ) + lerpResult836 ).rgb;
		}

		ENDCG
	}
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18900
870;518;1297;592;4216.645;1719.477;7.0379;True;True
Node;AmplifyShaderEditor.CommentaryNode;152;-2591.1,-268.3656;Inherit;False;806.7053;339.0518;Depth Fade;5;3;2;89;1;166;Depth;1,1,1,1;0;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;166;-2562.643,-216.1247;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenPosInputsNode;2;-2324.711,-110.8601;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenDepthNode;1;-2333.499,-194.9639;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;3;-2080.501,-147.2638;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;89;-1912.103,-144.6481;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;769;-684.6982,-457.4895;Inherit;False;1036.188;456.9082;Waves;11;818;829;814;475;477;813;721;17;23;896;897;Waves;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;897;-676.8011,-113.1025;Inherit;False;Property;_Wavestime;Waves time;6;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;794;-1623.57,-115.0285;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;721;-671.6141,-397.4558;Inherit;False;Property;_Wavestile;Waves tile;7;0;Create;True;0;0;0;False;0;False;1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;813;-675.5449,-286.9348;Inherit;False;Property;_WavesNormalscale;Waves Normal scale;9;0;Create;True;0;0;0;False;0;False;0.3;0.3;0.1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;896;-396.8011,-108.1025;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;816;-1340.244,-243.628;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;477;-492.2018,-415.9959;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;818;-676.8306,-205.7741;Inherit;False;Property;_Surfclamp;Surf clamp;30;0;Create;True;0;0;0;False;0;False;0.05;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;475;-187.3281,-409.9844;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.04,0.03;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;814;-180.8896,-286.1524;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;829;-192.7838,-153.8921;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.03,0.04;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;17;6.788425,-208.0759;Inherit;True;Property;_WavesNormal;Waves Normal;8;0;Create;True;0;0;0;False;0;False;-1;None;0e2840867afdcbc4a88cb532e35985a7;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;23;8.601299,-422.5974;Inherit;True;Property;_WavesNormal2;Waves Normal 2;8;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Instance;17;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;234;-531.8716,-2591.205;Inherit;False;3028.814;623.0454;Rimlight;24;750;753;758;760;253;251;247;246;255;252;240;238;265;250;244;254;242;261;249;836;841;843;886;888;Rimlight+Specular;1,1,1,1;0;0
Node;AmplifyShaderEditor.BlendNormalsNode;826;464.5795,-282.0019;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;261;-435.4382,-2530.584;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;249;-474.011,-2242.57;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;153;-2831.974,159.5769;Inherit;False;2570.978;742.7954;Foam;26;338;113;438;712;335;713;115;688;690;735;333;686;111;706;684;116;364;106;704;375;365;363;800;797;793;668;Foam;1,1,1,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;242;-154.0119,-2306.569;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;505;-2846.487,-23.73637;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;363;-2809.215,495.5995;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;704;-2544.448,277.7944;Inherit;False;Property;_Foamtile;Foam tile;23;0;Create;True;0;0;0;False;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;365;-2529.285,495.5934;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;375;-2802.045,583.6836;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;106;-2318.988,220.5851;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;10,10;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;364;-2382.671,617.6251;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;895;532.0017,-1554.597;Inherit;False;1813.997;650.1569;Caustics and sparkles;18;890;885;729;722;867;724;105;22;894;893;737;736;862;860;891;872;898;899;Caustics and sparkles;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;254;-243.9667,-2088.857;Float;False;Property;_RimOffset;Rim Offset;16;0;Create;True;0;0;0;False;0;False;0.564;0.593;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;116;-2065.225,225.2435;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;898;635.2087,-1080.908;Inherit;False;Property;_Causticalignmin;Caustic align min;21;0;Create;True;0;0;0;False;0;False;-3;-3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;736;559.1447,-1188.314;Inherit;False;Property;_Caustictile;Caustic tile;20;0;Create;True;0;0;0;False;0;False;15;15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;684;-1961.96,529.1949;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;899;633.2087,-1007.908;Inherit;False;Property;_Causticalignmax;Caustic align max;22;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;706;-2127.041,707.4508;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.4;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;238;113.4446,-2454.887;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;768;547.6741,277.7557;Inherit;False;1624.214;549.7675;Reflections;15;210;840;228;226;227;224;225;222;400;834;221;219;218;216;848;Reflections;0.2311321,1,0.8462238,1;0;0
Node;AmplifyShaderEditor.WireNode;902;-1044.726,-1184.698;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;244;68.10379,-2290.569;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;893;842.1371,-1073.62;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;-3;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;737;751.1449,-1220.314;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;111;-1518.495,342.7661;Float;False;Property;_FoamDepth;Foam Depth;26;0;Create;True;0;0;0;False;0;False;1.5;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;250;99.21306,-2066.926;Float;False;Property;_RimStr;Rim Str;17;0;Create;True;0;0;0;False;0;False;0.95;0.95;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;686;-1662.034,682.9836;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;668;-1840.211,207.9578;Inherit;True;Property;_WaterMask;Water Mask;28;0;Create;True;0;0;0;False;0;False;-1;None;f8d49aff2ed9bac4c825ad2dfac48383;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TransformDirectionNode;216;559.6752,346.792;Inherit;False;Tangent;View;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;894;1007.201,-1213.656;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SmoothstepOpNode;265;422.1218,-2289.372;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.42;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;735;-1326.646,348.3019;Float;False;Property;_FoamFalloff;Foam Falloff;25;0;Create;True;0;0;0;False;0;False;1.15;1.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;839;627.4816,-14.4311;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;872;640.733,-1400.606;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;442;-1367.975,-143.1015;Inherit;False;582.403;245.8323;Foam edgeblending;3;441;443;437;;0.2830189,0.2830189,0.2830189,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;240;442.5474,-2534.417;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;690;-1423.684,683.6314;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;12;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;333;-1322.886,210.5276;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;891;640.7505,-1489.203;Inherit;False;Property;_Sparklestile;Sparkles tile;18;0;Create;True;0;0;0;False;0;False;25;25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;159;-782.3449,-949.2329;Inherit;False;790.0168;382.0366;Depth control;6;6;94;87;10;88;12;Depth coloring;0.245105,0.3316393,0.6415094,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;860;854.7328,-1446.606;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;25,25;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;115;-1160.813,220.1345;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1.15;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;150;-594.1747,-1556.204;Inherit;False;881.5901;415.5618;Distortion;6;358;96;165;98;164;97;Distortion;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;437;-1168.695,-94.58467;Inherit;False;3;0;FLOAT;200;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;713;-1207.122,573.4477;Inherit;False;Property;_Surf;Surf;29;0;Create;True;0;0;0;False;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;218;847.444,419.6754;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PannerNode;22;1161.592,-1214.136;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.03,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;443;-1306.775,23.48034;Inherit;False;Property;_FoamIntensity;Foam Intensity;27;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;252;780.1287,-2479.205;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;688;-1096.591,683.305;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;164;-550.0677,-1496.846;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;441;-998.9056,-94.62729;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;105;1361.156,-1241.477;Inherit;True;Property;_RCaustic;(R) Caustic;28;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;668;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;221;974.9671,549.1839;Inherit;False;True;True;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;255;1006.677,-2478.089;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-745.0161,-861.5046;Float;False;Property;_WaterDepth;Water Depth;12;0;Create;True;0;0;0;False;0;False;0;0.63;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;729;1609.5,-1342.491;Inherit;False;Property;_CausticSparklesStr;Caustic/Sparkles Str;19;0;Create;True;0;0;0;False;0;False;1;1.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;862;1094.732,-1478.606;Inherit;True;Property;_GSparkles;(G) Sparkles;28;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Instance;668;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;219;986.1284,639.313;Inherit;False;Property;_Reflectpos;Reflect pos;34;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;335;-1007.136,234.8879;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1.2;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;760;1188,-2514.831;Inherit;False;Property;_Specular;Specular;14;0;Create;True;0;0;0;False;0;False;-0.624;-0.624;-1;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;834;783.8099,716.8914;Inherit;False;True;True;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;712;-954.7236,533.95;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;97;-351.2744,-1257.604;Float;False;Property;_Distortion;Distortion;10;0;Create;True;0;0;0;False;0;False;0.5;0.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;758;1482.963,-2506.949;Inherit;False;3;0;FLOAT;-0.7;False;1;FLOAT;1.25;False;2;FLOAT;0.45;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;770;-2292.073,-429.8392;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;400;1047.72,723.8236;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-535.0668,-748.8902;Float;False;Property;_WaterFalloff;Water Falloff;13;0;Create;True;0;0;0;False;0;False;-3.5;-3.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;98;-175.2739,-1327.498;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LightColorNode;232;1542.101,-878.7806;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;222;1198.273,577.784;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;885;1870.824,-1495.166;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;88;-529.6058,-879.7777;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;438;-837.7419,224.9452;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;165;-246.7201,-1439.712;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;224;1370.756,559.7478;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;848;1312.251,755.015;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;225;1351.523,363.8091;Inherit;True;Property;_ReflectionTex;_ReflectionTex;35;0;Create;True;0;0;0;False;0;False;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;226;1501.914,707.5624;Inherit;False;Property;_Reflectstr;Reflect str;33;0;Create;True;0;0;0;False;0;False;0.4;0.8;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;87;-347.5934,-880.2133;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;871;-748.9043,-493.942;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SignOpNode;753;1678.485,-2498.858;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;113;-653.8522,224.8192;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;890;2030.824,-1495.166;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;20;False;2;FLOAT;-0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;233;1772.526,-882.5358;Inherit;False;Lightcolor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;96;-20.07458,-1401.104;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;867;1491.497,-1036.837;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;853;1428.143,-659.6641;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.05;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;228;1674.175,330.1732;Inherit;False;Property;_Maincolor;Main color;0;0;Create;True;0;0;0;False;0;False;0.2117647,0.5529412,0.5843138,1;0.1742167,0.1816834,0.2735848,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;800;-614.108,756.6668;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;-2;False;2;FLOAT;8;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;247;913.7299,-2085.608;Inherit;False;233;Lightcolor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;246;620.1282,-2159.205;Float;False;Property;_RimColor;Rim Color;15;1;[HDR];Create;True;0;0;0;False;0;False;0.1333333,0.3058824,0.3098039,0;0.1333333,0.3058824,0.3098039,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;888;1975.206,-2250.151;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;227;1685.22,507.5157;Inherit;True;Global;_ReflectionTexinp;_ReflectionTexinp;26;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;750;1871.631,-2499.601;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;797;-763.8154,641.7365;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;843;1768.324,-2043.208;Inherit;False;Constant;_Float0;Float 0;29;0;Create;True;0;0;0;False;0;False;0.6;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;840;1835.212,710.3217;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;358;113.4855,-1403.028;Inherit;False;Global;_GrabScreen0;Grab Screen 0;20;0;Create;True;0;0;0;False;0;False;Object;-1;True;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;12;-281.5758,-783.0494;Float;False;Property;_DepthColor;Depth Color;11;0;Create;True;0;0;0;False;0;False;0,0.04313726,0.4039216,1;0.1686273,0.2156861,0.3019606,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;94;-137.6477,-881.3301;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;338;-490.0742,218.7641;Inherit;False;Foam;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;787;846.2321,70.93658;Inherit;False;Property;_TideebbStr;Tide-ebb Str;31;0;Create;True;0;0;0;False;0;False;0.11;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;886;2172.718,-2274.143;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;93;399.5118,-787.9296;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;841;1947.877,-2084.813;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0.1415094;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;251;892.1288,-2159.205;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;447;1994.328,-639.1523;Inherit;False;Property;_Foamcolor;Foam color;24;0;Create;True;0;0;0;False;0;False;0.7372549,0.7372549,0.7372549,0;0.735849,0.735849,0.735849,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;210;2015.466,489.9378;Inherit;False;3;0;COLOR;1,1,1,0;False;1;COLOR;1,1,1,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;339;2228.906,-709.9649;Inherit;False;338;Foam;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;793;-425.031,617.1488;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;901;2085.03,157.7283;Inherit;False;Constant;_Float1;Float 1;32;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;722;1905.271,-1308.674;Inherit;True;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;900;2268.584,160.0534;Inherit;False;Property;_Usereflection;Use reflection;32;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;False;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;836;2335.09,-2103.787;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;904;2016.705,-366.5318;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;453;2408.411,-657.6507;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightAttenuation;682;2494.771,-385.3416;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;835;687.1506,-281.3547;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;253;1457.827,-2194.804;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;781;1190.424,-174.4631;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;792;2404.819,-539.7777;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;724;2112.232,-1308.303;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;903;1527.43,-469.4771;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;683;2834.524,-497.8827;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;775;1355.5,-259.6469;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;259;2605.865,-707.1708;Inherit;False;6;6;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;552;3248.475,-732.1378;Half;False;True;-1;6;ASEMaterialInspector;0;0;CustomLighting;KumaBeer/Anime_water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Translucent;0.5;True;False;0;False;Opaque;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;0;32;0;35;False;1;False;0;5;False;-1;10;False;-1;0;5;False;-1;2;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;1;0;166;0
WireConnection;3;0;1;0
WireConnection;3;1;2;4
WireConnection;89;0;3;0
WireConnection;794;0;89;0
WireConnection;896;0;897;0
WireConnection;816;0;794;0
WireConnection;477;0;721;0
WireConnection;475;0;477;0
WireConnection;475;1;896;0
WireConnection;814;0;813;0
WireConnection;814;1;818;0
WireConnection;814;2;816;0
WireConnection;829;0;477;0
WireConnection;829;1;896;0
WireConnection;17;1;829;0
WireConnection;17;5;814;0
WireConnection;23;1;475;0
WireConnection;23;5;814;0
WireConnection;826;0;23;0
WireConnection;826;1;17;0
WireConnection;261;0;826;0
WireConnection;242;0;261;0
WireConnection;242;1;249;0
WireConnection;505;0;242;0
WireConnection;365;0;363;0
WireConnection;375;0;505;0
WireConnection;106;0;704;0
WireConnection;364;0;365;0
WireConnection;364;1;375;0
WireConnection;116;0;106;0
WireConnection;116;1;364;0
WireConnection;684;0;89;0
WireConnection;706;0;363;0
WireConnection;902;0;2;0
WireConnection;244;0;242;0
WireConnection;244;1;254;0
WireConnection;893;0;794;0
WireConnection;893;1;898;0
WireConnection;893;2;899;0
WireConnection;737;0;736;0
WireConnection;686;0;684;0
WireConnection;686;1;706;0
WireConnection;668;1;116;0
WireConnection;216;0;826;0
WireConnection;894;0;737;0
WireConnection;894;1;893;0
WireConnection;265;0;244;0
WireConnection;265;1;250;0
WireConnection;265;2;254;0
WireConnection;872;0;902;0
WireConnection;872;1;242;0
WireConnection;240;0;261;0
WireConnection;240;1;238;0
WireConnection;690;0;686;0
WireConnection;333;0;89;0
WireConnection;333;1;111;0
WireConnection;333;2;668;3
WireConnection;860;0;891;0
WireConnection;860;1;872;0
WireConnection;115;0;333;0
WireConnection;115;1;735;0
WireConnection;437;2;794;0
WireConnection;218;0;216;0
WireConnection;218;1;839;0
WireConnection;22;0;894;0
WireConnection;252;0;240;0
WireConnection;252;1;265;0
WireConnection;688;0;690;0
WireConnection;441;0;437;0
WireConnection;441;1;443;0
WireConnection;105;1;22;0
WireConnection;221;0;218;0
WireConnection;255;0;252;0
WireConnection;862;1;860;0
WireConnection;335;0;115;0
WireConnection;834;0;826;0
WireConnection;712;0;688;0
WireConnection;712;2;713;0
WireConnection;758;0;760;0
WireConnection;758;1;255;0
WireConnection;770;0;166;1
WireConnection;770;1;166;2
WireConnection;400;0;834;0
WireConnection;400;1;97;0
WireConnection;98;0;826;0
WireConnection;98;1;97;0
WireConnection;222;0;221;0
WireConnection;222;1;219;0
WireConnection;885;0;862;2
WireConnection;885;1;668;3
WireConnection;885;2;105;1
WireConnection;885;3;729;0
WireConnection;88;0;89;0
WireConnection;88;1;6;0
WireConnection;438;0;335;0
WireConnection;438;1;441;0
WireConnection;438;2;712;0
WireConnection;165;0;164;0
WireConnection;224;0;770;0
WireConnection;224;1;222;0
WireConnection;224;2;400;0
WireConnection;848;0;89;0
WireConnection;87;0;88;0
WireConnection;87;1;10;0
WireConnection;871;0;794;0
WireConnection;753;0;758;0
WireConnection;113;0;438;0
WireConnection;890;0;885;0
WireConnection;233;0;232;0
WireConnection;96;0;165;0
WireConnection;96;1;98;0
WireConnection;867;0;441;0
WireConnection;853;0;871;0
WireConnection;800;0;688;0
WireConnection;888;0;890;0
WireConnection;227;0;225;0
WireConnection;227;1;224;0
WireConnection;750;0;753;0
WireConnection;797;0;364;0
WireConnection;840;0;244;0
WireConnection;840;1;226;0
WireConnection;840;2;848;0
WireConnection;358;0;96;0
WireConnection;94;0;87;0
WireConnection;338;0;113;0
WireConnection;886;0;750;0
WireConnection;886;1;888;0
WireConnection;93;0;12;0
WireConnection;93;1;358;0
WireConnection;93;2;94;0
WireConnection;841;0;247;0
WireConnection;841;1;227;0
WireConnection;841;2;843;0
WireConnection;251;0;246;0
WireConnection;210;0;228;0
WireConnection;210;1;227;0
WireConnection;210;2;840;0
WireConnection;793;0;794;0
WireConnection;793;1;797;0
WireConnection;793;2;800;0
WireConnection;722;0;105;1
WireConnection;722;1;729;0
WireConnection;722;2;853;0
WireConnection;722;3;867;0
WireConnection;722;4;232;2
WireConnection;900;1;901;0
WireConnection;900;0;210;0
WireConnection;836;1;841;0
WireConnection;836;2;886;0
WireConnection;904;0;233;0
WireConnection;453;0;339;0
WireConnection;453;1;447;0
WireConnection;835;0;826;0
WireConnection;253;0;255;0
WireConnection;253;1;251;0
WireConnection;253;2;247;0
WireConnection;781;0;365;0
WireConnection;781;1;839;2
WireConnection;781;2;787;0
WireConnection;792;0;793;0
WireConnection;724;0;722;0
WireConnection;903;0;93;0
WireConnection;683;0;903;0
WireConnection;683;1;682;0
WireConnection;683;2;904;0
WireConnection;775;0;835;1
WireConnection;775;1;781;0
WireConnection;259;0;253;0
WireConnection;259;1;900;0
WireConnection;259;2;453;0
WireConnection;259;3;724;0
WireConnection;259;4;792;0
WireConnection;259;5;836;0
WireConnection;552;2;259;0
WireConnection;552;13;683;0
WireConnection;552;11;775;0
ASEEND*/
//CHKSM=62A9709759B7A7A85C11FF6E100ED640F1C3F4E2