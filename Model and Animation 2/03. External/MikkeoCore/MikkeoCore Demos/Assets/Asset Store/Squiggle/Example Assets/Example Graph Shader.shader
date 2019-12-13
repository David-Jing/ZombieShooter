// Shader created with Shader Forge v1.19 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.19;sub:START;pass:START;ps:flbk:,iptp:1,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:3138,x:33379,y:32572,varname:node_3138,prsc:2|emission-3404-OUT;n:type:ShaderForge.SFN_VertexColor,id:1106,x:32594,y:32699,varname:node_1106,prsc:2;n:type:ShaderForge.SFN_TexCoord,id:5363,x:32273,y:32948,varname:node_5363,prsc:2,uv:1;n:type:ShaderForge.SFN_Tex2d,id:3263,x:33000,y:32895,ptovrint:False,ptlb:Main Texture,ptin:_MainTexture,varname:node_3263,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-2815-OUT;n:type:ShaderForge.SFN_ValueProperty,id:5285,x:32464,y:33116,ptovrint:False,ptlb:YOffset,ptin:_YOffset,varname:node_5285,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Code,id:2815,x:32662,y:33097,varname:node_2815,prsc:2,code:cgBlAHQAdQByAG4AIABmAGwAbwBhAHQAMgAoAEEALAAgAEIAKQA7AA==,output:1,fname:CreateUV,width:384,height:132,input:0,input:0,input_1_label:A,input_2_label:B|A-6889-OUT,B-5285-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:4492,x:32930,y:32592,ptovrint:False,ptlb:Vertex Colors Enabled,ptin:_VertexColorsEnabled,varname:node_4492,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-6871-OUT,B-1106-RGB;n:type:ShaderForge.SFN_Vector3,id:6871,x:32618,y:32529,varname:node_6871,prsc:2,v1:1,v2:1,v3:1;n:type:ShaderForge.SFN_Multiply,id:3404,x:33003,y:32708,varname:node_3404,prsc:2|A-4492-OUT,B-3263-RGB;n:type:ShaderForge.SFN_Length,id:6889,x:32488,y:32948,varname:node_6889,prsc:2|IN-5363-UVOUT;proporder:4492-3263-5285;pass:END;sub:END;*/

Shader "Squiggle/Example Graph Shader" {
    Properties {
        [MaterialToggle] _VertexColorsEnabled ("Vertex Colors Enabled", Float ) = 1
        _MainTexture ("Main Texture", 2D) = "white" {}
        _YOffset ("YOffset", Float ) = 0
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
            "PreviewType"="Plane"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _MainTexture; uniform float4 _MainTexture_ST;
            uniform float _YOffset;
            float2 CreateUV( float A , float B ){
            return float2(A, B);
            }
            
            uniform fixed _VertexColorsEnabled;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord1 : TEXCOORD1;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv1 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv1 = v.texcoord1;
                o.vertexColor = v.vertexColor;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
/////// Vectors:
////// Lighting:
////// Emissive:
                float2 node_2815 = CreateUV( length(i.uv1) , _YOffset );
                float4 _MainTexture_var = tex2D(_MainTexture,TRANSFORM_TEX(node_2815, _MainTexture));
                float3 emissive = (lerp( float3(1,1,1), i.vertexColor.rgb, _VertexColorsEnabled )*_MainTexture_var.rgb);
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
