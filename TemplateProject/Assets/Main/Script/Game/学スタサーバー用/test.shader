Shader "CustomRenderTexture/test"
{
   SubShader  
    {
        Pass  
        {
            // 両面描画設定  
            Cull Off  

            CGPROGRAM  
            #pragma vertex vert_img  
            #pragma fragment frag  

            #include "UnityCG.cginc"  

            fixed4 frag (v2f_img i) : SV_Target  
            {
                // 筆の色設定  
                return fixed4(1, 1, 1, 1);  
            }
            ENDCG  
        }
    } 
}
