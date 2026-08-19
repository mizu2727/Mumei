using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// VersionTextを表示するためのクラス
/// </summary>
public class ViewVersionText : MonoBehaviour
{
    [Header("VersionText")]
    [SerializeField] private Text versionText;

    /// <summary>
    /// VersionTextの文字列
    /// </summary>
    private const string kVersion = "Ver Demo 1. 0. 24";

    void Start()
    {
        //VersionTextにバージョン番号を設定
        versionText.text = kVersion;
    }
}
