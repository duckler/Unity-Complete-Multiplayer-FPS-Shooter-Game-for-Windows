// Reference:
//		http://wiki.unity3d.com/index.php?title=AlphaSelfIllum

Shader "Character Armor" {
	Properties {
		_Color ("Color Tint (A = Opacity)", Color) = (1,1,1,1)
		//_MainTex ("Texture (A = Transparency)", 2D) = ""
	}

	SubShader {
		Tags {Queue = Transparent}
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass {
			//SetTexture[_MainTex] {Combine texture * constant ConstantColor[_Color]}
			SetTexture[_MainTex] {Combine constant ConstantColor[_Color]}
		}
	}

}