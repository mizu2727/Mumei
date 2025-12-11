
/// <summary>
/// Shader "メニュー名/シェーダー名"
/// シェーダーの情報
/// </summary>
Shader "Unlit/TestUnlitShader"
{
    /// <summary>
    /// Properties
    /// UnityのInspector上でやり取りをするプロパティ情報
    /// マテリアルのInspectorウィンドウ上に表示され、スクリプト上からも設定できる
    /// </summary>
    Properties
    {

        _Color ("Main Color", Color) = (1,1,1,1) // Color プロパティー (デフォルトは白)   a____
    }

    /// <summary>
    /// SubShader
    /// シェーダーの主な処理はこの中に記述する
    /// 複数書くことも可能が、基本は一つ
    /// </summary>
    SubShader
    {
        /// <summary>
        /// Pass
        /// 1つのオブジェクトの1度の描画で行う処理をここに書く
        /// 複数書くことも可能
        /// </summary>
        Pass
        {
            //プログラムを書き始めるという宣言
            CGPROGRAM

            //関数宣言
            //"vert" 関数を頂点シェーダー使用する宣言
            #pragma vertex vert

            //"frag" 関数をフラグメントシェーダーと使用する宣言
            #pragma fragment frag

            //変数宣言
            fixed4 _Color; // マテリアルからのカラー   a____

            //頂点シェーダー
            float4 vert (float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }

            //フラグメントシェーダー
            fixed4 frag () : SV_Target
            {
                return _Color;   //a____
            }

            //プログラムを書き終わるという宣言
            ENDCG   
        }
    }
}
