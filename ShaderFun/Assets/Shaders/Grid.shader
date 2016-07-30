// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "SketchPunk/Grid" {
	Properties {
		_GridThickness ("Grid Thickness", Float) = 0.01
		_GridSpacingX  ("Grid Spacing X", Float) = 1.0
		_GridSpacingY  ("Grid Spacing X", Float) = 1.0
		_GridOffsetX  ("Grid Offset X", Float) = 0
		_GridOffsetY  ("Grid Offset Y", Float) = 0
		_GridColor	("Grid Color", Color) = (1.0,1.0,1.0,1.0)
		_BaseColor	("Base Color", Color) = (0.0,0.0,0.0,0.0)
	}
	SubShader {
		Tags { "Queue"="Transparent" }

		Pass{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			uniform float _GridThickness;
			uniform float _GridSpacingX;
			uniform float _GridSpacingY;
			uniform float _GridOffsetX;
			uniform float _GridOffsetY;
			uniform float4 _GridColor;
			uniform float4 _BaseColor;

			struct vertexInput{
				float4 vertex : POSITION;
			};

			struct vertexOutput{
				float4 pos : SV_POSITION;
				float4 worldPos : TEXCOORD0;
			};

			vertexOutput vert(vertexInput input){
				vertexOutput o;
				o.pos = mul(UNITY_MATRIX_MVP,input.vertex);
				o.worldPos = mul(unity_ObjectToWorld,input.vertex);
				return o;
			}

			float4 frag(vertexOutput i) : COLOR{
				if( frac( (i.worldPos.x + _GridOffsetX) / _GridSpacingX ) < (_GridThickness / _GridSpacingX) ||
					frac( (i.worldPos.y + _GridOffsetY) / _GridSpacingY ) < (_GridThickness / _GridSpacingY) ||
					frac( (i.worldPos.z + _GridOffsetY) / _GridSpacingY ) < (_GridThickness / _GridSpacingY))
					return _GridColor;
				else return _BaseColor;
			}

			ENDCG
		}
	}
	//FallBack "Diffuse"
}
