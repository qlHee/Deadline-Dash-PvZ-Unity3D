//--------------------------------------------------------------------------------------------------------------------------------
// Cartoon FX
// (c) 2012-2020 Jean Moreno
//--------------------------------------------------------------------------------------------------------------------------------

Shader "Cartoon FX/Remaster/Particle Ubershader"
{
	Properties
	{
	//# Blending
	//#
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Blend Source", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Blend Destination", Float) = 10
		[KeywordEnumNoPrefix(Alpha Blending, _ALPHABLEND_ON, Alpha Blending Premultiplied, _ALPHAPREMULTIPLY_ON, Multiplicative, _ALPHAMODULATE_ON, Additive, _CFXR_ADDITIVE)] _BlendingType ("Blending Type", Float) = 0

	//# 
		[ToggleNoKeyword] _ZWrite ("Depth Write", Float) = 0
		[Toggle(_ALPHATEST_ON)] _UseAlphaClip ("Alpha Clipping (Cutout)", Float) = 0
	//# IF_KEYWORD _ALPHATEST_ON
		_Cutoff ("Cutoff Threshold", Range(0.001,1)) = 0.1
	//# END_IF
	
	//# --------------------------------------------------------
	
		[Toggle(_FADING_ON)] _UseSP ("Soft Particles", Float) = 0
	//# IF_KEYWORD _FADING_ON
		_SoftParticlesFadeDistanceNear ("Near Fade", Float) = 0
		_SoftParticlesFadeDistanceFar ("Far Fade", Float) = 1
	//# END_IF

	//# 

		[Toggle(_CFXR_EDGE_FADING)] _UseEF ("Edge Fade", Float) = 0
	//# IF_KEYWORD _CFXR_EDGE_FADING
		_EdgeFadePow ("Edge Fade Power", Float) = 1
	//# END_IF

	//# 

	//# ========================================================

	//# Effects
	//#

		[Toggle(_CFXR_DISSOLVE)] _UseDissolve ("Enable Dissolve", Float) = 0
	//# IF_KEYWORD _CFXR_DISSOLVE
		[NoScaleOffset] _DissolveTex ("Dissolve Texture", 2D) = "gray" {}
		_DissolveSmooth ("Dissolve Smoothing", Range(0.0001,0.5)) = 0.1
		[ToggleNoKeyword] _InvertDissolveTex ("Invert Dissolve Texture", Float) = 0
	//# END_IF

	//# --------------------------------------------------------

		[Toggle(_CFXR_UV_DISTORTION)] _UseUVDistortion ("Enable UV Distortion", Float) = 0
	//# IF_KEYWORD _CFXR_UV_DISTORTION
		
		[NoScaleOffset] _DistortTex ("Distortion Texture", 2D) = "gray" {}
		_DistortScrolling ("Scroll (XY) Tile (ZW)", Vector) = (0,0,1,1)
		[Toggle(_CFXR_UV2_DISTORTION)] _UseUV2Distortion ("Use UV2", Float) = 0
		_Distort ("Distortion Strength", Range(0,1.0)) = 0.1
		[ToggleNoKeyword] _FadeAlongU ("Fade along Y", Float) = 0
		[Toggle(_CFXR_UV_DISTORTION_ADD)] _UVDistortionAdd ("Add to base UV", Float) = 0
	//# END_IF

	//# ========================================================

	//# Colors
	//#

		[NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
		[Toggle(_CFXR_SINGLE_CHANNEL)] _SingleChannel ("Single Channel Texture", Float) = 0

	//# --------------------------------------------------------

		[KeywordEnum(Off,1x,2x)] _CFXR_OVERLAYTEX ("Enable Overlay Texture", Float) = 0
	//# IF_KEYWORD _CFXR_OVERLAYTEX_1X || _CFXR_OVERLAYTEX_2X
		[KeywordEnum(RGBA,RGB,A)] _CFXR_OVERLAYBLEND ("Overlay Blend Channels", Float) = 0
		[NoScaleOffset] _OverlayTex ("Overlay Texture", 2D) = "white" {}
		_OverlayTex_Scroll ("Overlay Scrolling / Scale", Vector) = (0.1,0.1,1,1)
	//# END_IF

	//# --------------------------------------------------------

		[Toggle(_FLIPBOOK_BLENDING)] _UseFB ("Flipbook Blending", Float) = 0

	//# --------------------------------------------------------

		[Toggle(_CFXR_SECONDCOLOR_LERP)] _UseSecondColor ("Secondary Vertex Color", Float) = 0
	//# IF_KEYWORD _CFXR_SECONDCOLOR_LERP
		[NoScaleOffset] _SecondColorTex ("Second Color Map", 2D) = "black" {}
		_SecondColorSmooth ("Second Color Smoothing", Range(0.0001,0.5)) = 0.2
	//# END_IF

	//# --------------------------------------------------------

		[Toggle(_CFXR_FONT_COLORS)] _UseFontColor ("Use Font Colors", Float) = 0

//	//# --------------------------------------------------------
//
//	[Toggle(_CFXR_GRADIENTMAP)] _UseGradientMap ("Gradient Map", Float) = 0
//	//# IF_KEYWORD _CFXR_GRADIENTMAP
//		[NoScaleOffset] _GradientMap ("Gradient Map", 2D) = "black" {}
//	//# END_IF

	//# --------------------------------------------------------

		[Toggle(_CFXR_HDR_BOOST)] _HdrBoost ("Enable HDR Multiplier", Float) = 0
	//# IF_KEYWORD _CFXR_HDR_BOOST
		 _HdrMultiply ("HDR Multiplier", Float) = 2
	//# END_IF


	// //# --------------------------------------------------------

//		[Toggle(_CFXR_AMBIENT_LIGHTING)] _AmbientLighting ("Enable SH (Ambient, Light Probes)", Float) = 0
//	//# IF_KEYWORD _CFXR_AMBIENT_LIGHTING
//		_AmbientColor ("Color Contribution", Range(0,1)) = 0.5
//		_AmbientIntensity ("Intensity", Range(0,1)) = 1.0
//	//# END_IF

	//# ========================================================
	//# Shadows
	//#

		[KeywordEnum(Off,On,CustomTexture)] _CFXR_DITHERED_SHADOWS ("Dithered Shadows", Float) = 0
	//# IF_KEYWORD _CFXR_DITHERED_SHADOWS_ON || _CFXR_DITHERED_SHADOWS_CUSTOMTEXTURE
		_ShadowStrength		("Shadows Strength Max", Range(0,1)) = 1.0
		//#	IF_KEYWORD _CFXR_DITHERED_SHADOWS_CUSTOMTEXTURE
		_DitherCustom		("Dithering 3D Texture", 3D) = "black" {}
		//#	END_IF
	//# END_IF

//		_ReceivedShadowsStrength ("Received Shadows Strength", Range(0,1)) = 0.5
	}
	
	Category
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
		}

		Blend [_SrcBlend] [_DstBlend], One One
		ZWrite [_ZWrite]
		Cull  Off

		//================================================================================================================================
		// Includes & Code

		CGINCLUDE

			#define CFXR_URP

			#include "UnityCG.cginc"
			#include "UnityStandardUtils.cginc"
			#include "AutoLight.cginc"

			CBUFFER_START(UnityPerMaterial)

			sampler2D _MainTex;
			sampler2D _OverlayTex;
			float4 _OverlayTex_Scroll;
			half _AmbientColor;
			half _AmbientIntensity;

			sampler2D _DissolveTex;
			half _InvertDissolveTex;
			half _DissolveSmooth;

			sampler2D _DistortTex;
			half4 _DistortScrolling;
			half _Distort;
			half _FadeAlongU;

			// sampler2D _GradientMap;

			sampler2D _SecondColorTex;
			half _SecondColorSmooth;

			half _HdrMultiply;

			half _ReceivedShadowsStrength;

			half _Cutoff;

			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			half _SoftParticlesFadeDistanceNear;
			half _SoftParticlesFadeDistanceFar;
			half _EdgeFadePow;

			float _ShadowStrength;
			sampler3D _DitherMaskLOD;
			sampler3D _DitherCustom;
			float4 _DitherCustom_TexelSize;

			CBUFFER_END

			// --------------------------------
			// Input/output

			struct appdata
			{
				float4 vertex		: POSITION;
				half4 color			: COLOR;
				float4 texcoord		: TEXCOORD0;	//xy = uv, zw = random
				float4 texcoord1	: TEXCOORD1;	//additional particle data: x = dissolve, y = animFrame
				float4 texcoord2	: TEXCOORD2;	//additional particle data: second color
		#if PASS_SHADOW_CASTER || _CFXR_EDGE_FADING || _CFXR_AMBIENT_LIGHTING
				float3 normal : NORMAL;
		#endif
			};

			// vertex to fragment
			struct v2f
			{
				float4 pos				: SV_POSITION;
				half4 color				: COLOR;
				float4 uv_random		: TEXCOORD0;	//uv + particle data
				float4 custom1			: TEXCOORD1;	//additional particle data
		#if _CFXR_SECONDCOLOR_LERP || _CFXR_FONT_COLORS
				float4 secondColor		: TEXCOORD2;
		#endif
		#if (defined(SOFTPARTICLES_ON) && defined(_FADING_ON))
				float4 projPos			: TEXCOORD3;
		#endif
		#if !PASS_SHADOW_CASTER
				UNITY_FOG_COORDS(4)		//note: does nothing if fog is not enabled
				// SHADOW_COORDS(4)
		#endif
			};

			// vertex to fragment (shadow caster)
			struct v2f_shadowCaster
			{
				V2F_SHADOW_CASTER_NOPOS
				half4 color				: COLOR;
				float4 uv_random		: TEXCOORD1;	//uv + particle data
				float4 custom1			: TEXCOORD2;	//additional particle data
			};

			// --------------------------------

			#include "CFXR.cginc"

			// --------------------------------
			// Vertex

		#if PASS_SHADOW_CASTER
			void vertex_program (appdata v, out v2f_shadowCaster o, out float4 opos : SV_POSITION)
		#else
			v2f vertex_program (appdata v)
		#endif
			{
		#if !PASS_SHADOW_CASTER
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
		#endif

				o.color = v.color;
				o.uv_random = v.texcoord;
				o.custom1 = v.texcoord1;
		#if _CFXR_SECONDCOLOR_LERP || _CFXR_FONT_COLORS
				o.secondColor = v.texcoord2;
		#endif

		/*
		#if _CFXR_AMBIENT_LIGHTING
			#ifdef UNITY_SAMPLE_FULL_SH_PER_PIXEL
				#undef UNITY_SAMPLE_FULL_SH_PER_PIXEL
			#endif
				half3 shColor = ShadeSHPerVertex(v.normal.xyz, half3(0,0,0));
			#ifdef UNITY_COLORSPACE_GAMMA
				shColor = LinearToGammaSpace(shColor);
			#endif
				shColor = lerp(Luminance(shColor), shColor, _AmbientColor);
				o.color.rgb = lerp(half3(1,1,1), shColor.rgb, _AmbientIntensity);
		#endif
		*/

				/*
		#if !PASS_SHADOW_CASTER
				// Shadows Receiving
				TRANSFER_SHADOW(o);
		#endif
				*/

		#if PASS_SHADOW_CASTER
				vert(v, o, opos);
		#else
				return vert(v, o);
		#endif
			}

			// --------------------------------
			// Fragment

		#if PASS_SHADOW_CASTER
			float4 fragment_program (v2f_shadowCaster i, UNITY_VPOS_TYPE vpos : VPOS) : SV_Target
		#else
			half4 fragment_program (v2f i) : SV_Target
		#endif
			{
				// ================================================================
				// UV Distortion

			#if _CFXR_UV_DISTORTION
				#if _CFXR_UV2_DISTORTION
					float2 uvDistortion = tex2D(_DistortTex, (i.custom1.xy * _DistortScrolling.zw + i.uv_random.zw + frac(_DistortScrolling.xy * _Time.yy)));
				#else
					float2 uvDistortion = tex2D(_DistortTex, (i.uv_random.xy * _DistortScrolling.zw + i.uv_random.zw + frac(_DistortScrolling.xy * _Time.yy)));
				#endif

				#if _CFXR_UV_DISTORTION_ADD
					uvDistortion = i.uv_random.xy + uvDistortion * _Distort;
				#else
					uvDistortion = lerp(i.uv_random.xy, uvDistortion, _Distort);
				#endif

				if (_FadeAlongU > 0)
				{
					uvDistortion = lerp(i.uv_random.xy, uvDistortion, i.uv_random.y * 0.5);
				}

				#define main_uv uvDistortion
			#else
				#define main_uv i.uv_random
			#endif

				// ================================================================
				// Color & Alpha

			#if _CFXR_SINGLE_CHANNEL
				half4 mainTex = half4(1, 1, 1, tex2D(_MainTex, main_uv.xy).r);
			#else
				half4 mainTex = tex2D(_MainTex, main_uv.xy);
			#endif

			#ifdef _FLIPBOOK_BLENDING
				#if _CFXR_SINGLE_CHANNEL
					half4 mainTex2 = tex2D(_MainTex, i.uv_random.zw).r;
				#else
					half4 mainTex2 = tex2D(_MainTex, i.uv_random.zw);
				#endif
					mainTex = lerp(mainTex, mainTex2, i.custom1.y);
			#endif

			#if _CFXR_OVERLAYTEX_1X
				float2 timeOffset = frac(_Time.yy * _OverlayTex_Scroll.xy);
				float2 overlayUv = ((i.uv_random.xy + i.uv_random.zw) * _OverlayTex_Scroll.zz) + timeOffset;
				half4 overlay = tex2D(_OverlayTex, overlayUv).r;
			#elif _CFXR_OVERLAYTEX_2X
				float2 timeOffset = frac(_Time.yy * _OverlayTex_Scroll.xy);
				float2 overlayUv = ((i.uv_random.xy + i.uv_random.zw) * _OverlayTex_Scroll.zz) + timeOffset;
				half4 overlay = tex2D(_OverlayTex, overlayUv).r;

				overlayUv = ((i.uv_random.xy + i.uv_random.wz) * _OverlayTex_Scroll.ww) + timeOffset;
				half4 overlay2 = tex2D(_OverlayTex, overlayUv).r;
				mainTex.a *= (overlay.r + overlay2.r) / 2.0;
			#endif

			#if _CFXR_OVERLAYTEX_1X || _CFXR_OVERLAYTEX_2X
				#if _CFXR_OVERLAYBLEND_A
				mainTex.a *= overlay.r;
				#elif _CFXR_OVERLAYBLEND_RGB
				mainTex.rgb *= overlay.rgb;
				#else
				mainTex.rgba *= overlay.rgba;
				#endif
			#endif
					
				/*
			#if _CFXR_GRADIENTMAP
				mainTex.rgb = tex2D(_GradientMap, mainTex.a).rgb;
			#endif
				*/

			#if _CFXR_FONT_COLORS
				half3 particleColor = mainTex.b * i.color.rgb + mainTex.g * i.custom1.rgb + mainTex.r * i.secondColor.rgb;
				half particleAlpha = mainTex.a * i.color.a;
			#else
				half3 particleColor = mainTex.rgb * i.color.rgb;
				half particleAlpha = mainTex.a * i.color.a;
			#endif
				
			#if _CFXR_SECONDCOLOR_LERP
				half secondColorMap = tex2D(_SecondColorTex, i.uv_random.xy).r;
				float time = lerp(-_SecondColorSmooth, 1+_SecondColorSmooth, i.secondColor.a);
				secondColorMap = smoothstep(secondColorMap - _SecondColorSmooth, secondColorMap + _SecondColorSmooth, time);
				particleColor.rgb += i.secondColor.rgb * secondColorMap;
			#endif

			#if _CFXR_HDR_BOOST
				#ifdef UNITY_COLORSPACE_GAMMA
					_HdrMultiply = LinearToGammaSpaceApprox(_HdrMultiply);
				#endif
				particleColor.rgb *= _HdrMultiply * GLOBAL_HDR_MULTIPLIER;
			#endif

				/*
			#if !PASS_SHADOW_CASTER
				// Shadows Receiving
				half shadows = SHADOW_ATTENUATION(i);
				particleColor.rgb *= saturate(shadows + (1.0 - _ReceivedShadowsStrength));
			#endif
				*/

				// ================================================================
				// Dissolve

			#if _CFXR_DISSOLVE
				half dissolveTex = tex2D(_DissolveTex, i.uv_random.xy).r;
				dissolveTex = _InvertDissolveTex <= 0 ? 1 - dissolveTex : dissolveTex;
				half dissolveTime = i.custom1.x;
			#else
				half dissolveTex = 0;
				half dissolveTime = 0;
			#endif

				// ================================================================
				//

			#if PASS_SHADOW_CASTER
				return frag(i, vpos, particleColor, particleAlpha, dissolveTex, dissolveTime);
			#else
				return frag(i, particleColor, particleAlpha, dissolveTex, dissolveTime);
			#endif
			}

		ENDCG

		//====================================================================================================================================
		// Universal Rendering Pipeline

		Subshader
		{
			Pass
			{
				Name "BASE_URP"
				Tags { "LightMode"="UniversalForward" }

				CGPROGRAM

				#pragma vertex vertex_program
				#pragma fragment fragment_program
				
				#pragma target 2.0
				
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				//#pragma multi_compile_fwdbase
				//#pragma multi_compile SHADOWS_SCREEN
				
				#pragma shader_feature _ _CFXR_SINGLE_CHANNEL
				#pragma shader_feature _ _CFXR_DISSOLVE
				#pragma shader_feature _ _CFXR_UV_DISTORTION
				#pragma shader_feature _ _CFXR_UV2_DISTORTION
				#pragma shader_feature _ _CFXR_UV_DISTORTION_ADD
				// #pragma shader_feature _ _CFXR_GRADIENTMAP
				#pragma shader_feature _ _CFXR_SECONDCOLOR_LERP _CFXR_FONT_COLORS
				#pragma shader_feature _ _CFXR_OVERLAYTEX_1X _CFXR_OVERLAYTEX_2X
				#pragma shader_feature _ _CFXR_OVERLAYBLEND_A _CFXR_OVERLAYBLEND_RGB
				#pragma shader_feature _ _CFXR_HDR_BOOST
				#pragma shader_feature _ _CFXR_EDGE_FADING
				// #pragma shader_feature _ _CFXR_AMBIENT_LIGHTING

				// Using the same keywords as Unity's Standard Particle shader to minimize project-wide keyword usage
				#pragma shader_feature _ _FLIPBOOK_BLENDING
				#pragma shader_feature _ _FADING_ON
				#pragma shader_feature _ _ALPHATEST_ON
				#pragma shader_feature _ _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON _CFXR_ADDITIVE


				ENDCG
			}

			//--------------------------------------------------------------------------------------------------------------------------------

			Pass
			{
				Name "ShadowCaster"
				Tags { "LightMode" = "ShadowCaster" }

				BlendOp Add
				Blend One Zero
				ZWrite On
				Cull Off

				CGPROGRAM

				#pragma multi_compile PASS_SHADOW_CASTER

				#pragma vertex vertex_program
				#pragma fragment fragment_program

				#pragma shader_feature _ _CFXR_SINGLE_CHANNEL
				#pragma shader_feature _ _CFXR_DISSOLVE
				#pragma shader_feature _ _CFXR_UV_DISTORTION
				#pragma shader_feature _ _CFXR_UV2_DISTORTION
				#pragma shader_feature _ _CFXR_UV_DISTORTION_ADD
				#pragma shader_feature _ _CFXR_OVERLAYTEX_1X _CFXR_OVERLAYTEX_2X
				#pragma shader_feature _ _CFXR_OVERLAYBLEND_A _CFXR_OVERLAYBLEND_RGB
				#pragma shader_feature _ _FLIPBOOK_BLENDING

				#pragma shader_feature _ _ALPHATEST_ON
				#pragma shader_feature _ _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON _CFXR_ADDITIVE

				#pragma multi_compile_shadowcaster
				#pragma shader_feature _ _CFXR_DITHERED_SHADOWS_ON _CFXR_DITHERED_SHADOWS_CUSTOMTEXTURE

			#if _CFXR_DITHERED_SHADOWS_ON || _CFXR_DITHERED_SHADOWS_CUSTOMTEXTURE
				#pragma target 3.0		//needed for VPOS
			#endif

				ENDCG
			}
		}

		//====================================================================================================================================
		// Built-in Rendering Pipeline

		SubShader
		{
			Pass
			{
				Name "BASE"
				Tags { "LightMode"="ForwardBase" }

				CGPROGRAM

				#pragma vertex vertex_program
				#pragma fragment fragment_program
				
				#pragma target 2.0
				
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				//#pragma multi_compile_fwdbase
				//#pragma multi_compile SHADOWS_SCREEN
				
				#pragma shader_feature _ _CFXR_SINGLE_CHANNEL
				#pragma shader_feature _ _CFXR_DISSOLVE
				#pragma shader_feature _ _CFXR_UV_DISTORTION
				#pragma shader_feature _ _CFXR_UV2_DISTORTION
				#pragma shader_feature _ _CFXR_UV_DISTORTION_ADD
				// #pragma shader_feature _ _CFXR_GRADIENTMAP
				#pragma shader_feature _ _CFXR_SECONDCOLOR_LERP _CFXR_FONT_COLORS
				#pragma shader_feature _ _CFXR_OVERLAYTEX_1X _CFXR_OVERLAYTEX_2X
				#pragma shader_feature _ _CFXR_OVERLAYBLEND_A _CFXR_OVERLAYBLEND_RGB
				#pragma shader_feature _ _CFXR_HDR_BOOST
				#pragma shader_feature _ _CFXR_EDGE_FADING
				// #pragma shader_feature _ _CFXR_AMBIENT_LIGHTING

				// Using the same keywords as Unity's Standard Particle shader to minimize project-wide keyword usage
				#pragma shader_feature _ _FLIPBOOK_BLENDING
				#pragma shader_feature _ _FADING_ON
				#pragma shader_feature _ _ALPHATEST_ON
				#pragma shader_feature _ _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON _CFXR_ADDITIVE


				ENDCG
			}

			//--------------------------------------------------------------------------------------------------------------------------------

			Pass
			{
				Name "ShadowCaster"
				Tags { "LightMode" = "ShadowCaster" }

				BlendOp Add
				Blend One Zero
				ZWrite On
				Cull Off

				CGPROGRAM

				#pragma multi_compile PASS_SHADOW_CASTER

				#pragma vertex vertex_program
				#pragma fragment fragment_program

				#pragma shader_feature _ _CFXR_SINGLE_CHANNEL
				#pragma shader_feature _ _CFXR_DISSOLVE
				#pragma shader_feature _ _CFXR_UV_DISTORTION
				#pragma shader_feature _ _CFXR_UV2_DISTORTION
				#pragma shader_feature _ _CFXR_UV_DISTORTION_ADD
				#pragma shader_feature _ _CFXR_OVERLAYTEX_1X _CFXR_OVERLAYTEX_2X
				#pragma shader_feature _ _CFXR_OVERLAYBLEND_A _CFXR_OVERLAYBLEND_RGB
				#pragma shader_feature _ _FLIPBOOK_BLENDING

				#pragma shader_feature _ _ALPHATEST_ON
				#pragma shader_feature _ _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON _CFXR_ADDITIVE

				#pragma multi_compile_shadowcaster
				#pragma shader_feature _ _CFXR_DITHERED_SHADOWS_ON _CFXR_DITHERED_SHADOWS_CUSTOMTEXTURE

			#if _CFXR_DITHERED_SHADOWS_ON || _CFXR_DITHERED_SHADOWS_CUSTOMTEXTURE
				#pragma target 3.0		//needed for VPOS
			#endif

				ENDCG
			}
		}
	}
	
	CustomEditor "CartoonFX.MaterialInspector"
}

