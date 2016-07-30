// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "SketchPunk/Rim" {
	Properties {
		_Color("Color", Color) = (1.0,1.0,1.0,1.0)
		_SpecColor("Specular Color", Color) = (1.0,1.0,1.0,1.0)
		_Shininess("Shininess",float) = 10
		_RimColor("Rim Color", Color) = (1.0,1.0,1.0,1.0)
		_RimPower("Rim Power", Range(0.1,10.0)) = 3.0
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
			uniform float4 _RimColor;
			uniform float _RimPower;


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
				float4 posWorld : TEXCOORD0;
				float3 normalDir : TEXCOORD1;
			};

			//Functions
			vertexOutput vert(vertexInput v){
				vertexOutput o;

				o.pos =  mul(UNITY_MATRIX_MVP, v.vertex); //Convert position to something Unity understand. 
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.normalDir = normalize( mul( float4(v.normal,0.0), unity_WorldToObject).xyz );

				return o;
			}

			float4 frag(vertexOutput i) : COLOR{

				float3 normalDirection = i.normalDir;
				float3 viewDirection = normalize( _WorldSpaceCameraPos.xyz - i.posWorld.xyz );
				float3 lightDirection = normalize( _WorldSpaceCameraPos.xyz );
				float atten = 1.0;

				//Lighting
				float3 diffuseReflection = atten * _LightColor0.xyz * saturate ( dot( normalDirection,lightDirection ));
				float3 specularReflection = diffuseReflection * pow( saturate( dot( reflect(-lightDirection,normalDirection),viewDirection)), _Shininess);

				//Rim Light
				float3 rim = 1 - saturate( dot( normalize(viewDirection), normalDirection) );
				float3 rimLighting = atten * _LightColor0.xyz * _RimColor * saturate( dot(normalDirection, lightDirection) )  * pow(rim,_RimPower) ;
				float3 lightFinal = rimLighting + diffuseReflection + specularReflection + UNITY_LIGHTMODEL_AMBIENT;

				return float4(lightFinal * _Color.xyz, 1.0);
			}

			ENDCG
		}
	}
	//FallBack "Diffuse"
}