Shader "UFXCase/RoundRectangle"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white"{}
        _Color ("Color", Color) = (1,0,1,1)
        _EdgeColor("Edge Color", Color) = (0, 1, 1, 1)
        _EdgeRadius("Round Radius", Float) = 10
        [ShowAsVector2] _Rect("Rect Size", Vector) = (100, 200, 0, 0)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _EdgeColor;
        half _EdgeRadius;
        fixed2 _Rect;
        half _Glossiness;
        half _Metallic;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            float2 pix = IN.uv_MainTex * _Rect;
            float2 leftdown = float2(_EdgeRadius, _EdgeRadius);
            float2 leftup = float2(_EdgeRadius, _Rect.y - _EdgeRadius);
            float2 rightdown = float2(_Rect.x - _EdgeRadius, _EdgeRadius);
            float2 rightup = float2(_Rect.x - _EdgeRadius, _Rect.y - _EdgeRadius);

            float v = sign(pix.x-leftdown.x) + sign(rightdown.x-pix.x) + sign(pix.y-leftdown.y) + sign(leftup.y-pix.y);
            c = lerp(_Color, _EdgeColor, step(v, 3.9));

            c.a = IN.uv_MainTex.y;
            c.a = lerp(0, c.a, step(sign(leftdown.x - pix.x) + sign(leftdown.y - pix.y) + sign(length(pix - leftdown) - _EdgeRadius), 2.9));
            c.a = lerp(0, c.a, step(sign(leftup.x - pix.x) + sign(pix.y - leftup.y) + sign(length(pix - leftup) - _EdgeRadius), 2.9));
            c.a = lerp(0, c.a, step(sign(pix.x - rightdown.x) + sign(rightdown.y - pix.y) + sign(length(pix - rightdown) - _EdgeRadius), 2.9));
            c.a = lerp(0, c.a, step(sign(pix.x - rightup.x) + sign(pix.y - rightup.y) + sign(length(pix - rightup) - _EdgeRadius), 2.9));

            /*
            bool pix.x < leftdown.x || pix.x > rightdown.x || pix.y < leftdown.y || pix.y > leftup.y;
            if () {
                c = _EdgeColor;
            }
            else {
                c = _Color;
            }
            */

            //if (pix.x < leftdown.x && pix.y < leftdown.y) {
            //    // left down
            //    if (length(pix - leftdown) > _EdgeRadius) {
            //        c.a = 0;
            //    }
            //}
            //else if (pix.x < leftup.x && pix.y > leftup.y) {
            //    // left up
            //    if (length(pix - leftup) > _EdgeRadius) {
            //        c.a = 0;
            //    }
            //}
            //else if (pix.x > rightdown.x && pix.y < rightdown.y) {
            //    // right down
            //    if (length(pix - rightdown) > _EdgeRadius) {
            //        c.a = 0;
            //    }
            //}
            //else if (pix.x > rightup.x && pix.y > rightup.y) {
            //    // right up
            //    if (length(pix - rightup) > _EdgeRadius) {
            //        c.a = 0;
            //    }
            //}

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = 0;
            o.Smoothness = 0;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
