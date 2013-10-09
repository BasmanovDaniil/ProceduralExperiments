Shader "Custom/Khrushchyovka" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_GlassColor ("Glass Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_SpecTex ("Specular (RGB)", 2D) = "gray" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf BlinnPhong

		sampler2D _MainTex, _SpecTex;
		fixed4 _Color, _GlassColor;
		half _Shininess;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 main = tex2D (_MainTex, IN.uv_MainTex);
			half4 spec = tex2D(_SpecTex, IN.uv_MainTex);
			o.Albedo = main.rgb * _Color.rgb + spec.rgb * _GlassColor.rgb;
			o.Gloss = spec.rgb;
			o.Specular = _Shininess;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
