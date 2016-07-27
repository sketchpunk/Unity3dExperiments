Shader "SketchPunk/FlatColor" {
	Properties {
		_Color("Color", Color) = (1.0,1.0,1.0,1.0)
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			//Vars
			uniform float4 _Color;

			struct vertexInput{
				float4 vertex : POSITION;	
			};

			struct vertexOutput{
				float4 pos : SV_POSITION; //REQUIRED
			};

			//Functions
			vertexOutput vert(vertexInput v){
				vertexOutput o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex); //Covert position to something Unity understand. 
				return o;
			}

			float4 frag(vertexOutput i) : COLOR{
				return _Color;
			}

			ENDCG
		}
	}
	//FallBack "Diffuse"
}
