// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "SketchPunk/Specular" {
	Properties {
		_Color("Color", Color) = (1.0,1.0,1.0,1.0)
		_SpecColor("Specular Color", Color) = (1.0,1.0,1.0,1.0)
		_Shininess("Shininess",float) = 10
	}
	SubShader {
		Pass {
			Tags { "LightMode" = "ForwardBase" }  //Need to mention which light to use else it will not render in real time

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			//Vars
			uniform float4 _Color;
			uniform float4 _SpecColor;
			uniform float _Shininess;


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

				float3 normalDirection = normalize( mul( float4(v.normal,0.0) , unity_WorldToObject).xyz );
				float3 viewDirection = normalize( float3( float4(_WorldSpaceCameraPos.xyz,1.0) - mul(unity_ObjectToWorld, v.vertex).xyz ) );    //Calc Distance between vertex and camera, can also be used as rotation vector
				float atten = 1.0; //Distance between light and the source

				float3 lightDirection = normalize( _WorldSpaceLightPos0.xyz );
				float3 diffuseReflection = atten * _LightColor0.xyz * max(0.0, dot(normalDirection,lightDirection)); //If facing away from light should get a zero, but facing it gets a 1
				//float3 specularReflection = reflect( -lightDirection, normalDirection); 
				//float3 specularReflection = dot( reflect( -lightDirection, normalDirection), viewDirection); 
				//float3 specularReflection =  max(0.0, dot(normalDirection,lightDirection)) * max(0.0,dot( reflect( -lightDirection, normalDirection), viewDirection)); 
				float3 specularReflection = atten * _SpecColor.rgb *  max(0.0, dot(normalDirection,lightDirection)) * pow( max(0.0,dot( reflect( -lightDirection, normalDirection), viewDirection)), _Shininess);
				float3 lightFinal = diffuseReflection + specularReflection + UNITY_LIGHTMODEL_AMBIENT;

				//o.color = float4(specularReflection,1.0);
				o.color = float4(lightFinal * _Color.rgb,1.0);

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