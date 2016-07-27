Shader "SketchPunk/Ambient" {
	Properties {
		_Color("Color", Color) = (1.0,1.0,1.0,1.0)
	}
	SubShader {
		Pass {
			Tags { "LightMode" = "ForwardBase" }  //Need to mention which light to use else it will not render in real time

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			//Vars
			uniform float4 _Color;

			//Unity defined vars
			uniform float4 _LightColor0;
			//float4x4 _Object2World;		//These 3 predefined in unity 4.0, need it for unity 3.5
			//float4x4 _World2Object;
			//float4 _WorldSpaceLightPos0;

			struct vertexInput{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct vertexOutput{
				float4 pos : SV_POSITION; //REQUIRED
				float4 color : COLOR;
			};

			//Functions
			vertexOutput vert(vertexInput v){
				vertexOutput o;

				float3 normalDirection = normalize( mul( float4(v.normal,0.0) , _World2Object).xyz );
				float3 lightDirection = normalize( _WorldSpaceLightPos0.xyz );
				float atten = 1.0; //Distance between light and the source
				 
				float3 disffuseDirection = atten * _LightColor0.xyz  * max(0.0 , dot(normalDirection, lightDirection) ); //calc our light
				float3 lightFinal = disffuseDirection + UNITY_LIGHTMODEL_AMBIENT.xyz;

				o.color = float4(lightFinal * _Color.rgb,1.0); //float4(normalDirection,1.0); //float4(v.normal,1.0);
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex); //Convert position to something Unity understand. 
				return o;
			}

			float4 frag(vertexOutput i) : COLOR{
				return i.color;
			}

			ENDCG
		}
	}
	//FallBack "Diffuse"
}