using UnityEngine;

public partial class PauseController
{
    /// <summary>
    /// 「リタイア」ボタン押下
    /// </summary>
    public void OnClickedViewRetirePanelButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //ポーズパネルを非表示にし、リタイアパネルを表示する
        isRetirePanel = true;
        ChangeViewRetirePanel();

        isPause = false;
        ChangeViewPausePanel();
    }

    /// <summary>
    /// 「ステージ選択へ戻る」ボタン押下
    /// </summary>
    public void OnClickedViewReturnToSelectStagePanelButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //リタイアパネルを非表示にし、ステージ選択へ戻るパネルを表示する
        isReturnToSelectStagePanel = true;
        ChangeReturnToSelectStagePanel();

        isRetirePanel = false;
        ChangeViewRetirePanel();
    }

    /// <summary>
    /// 「タイトルへ戻る」ボタン押下
    /// </summary>
    public void OnClickedReturnToTitleButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //リタイアパネルを非表示にし、タイトルへ戻るパネルを表示する
        isReturnToTitlePanel = true;
        ChangeReturnToTitlePanel();

        isRetirePanel = false;
        ChangeViewRetirePanel();
    }

    /// <summary>
    /// リタイアパネルの表示/非表示
    /// </summary>
    private void ChangeViewRetirePanel()
    {
        if (isRetirePanel)
        {
            //表示
            retirePanel.SetActive(true);
        }
        else
        {
            //非表示
            retirePanel.SetActive(false);
        }
    }

    /// <summary>
    /// ステージ選択へ戻るパネルの表示/非表示
    /// </summary>
    private void ChangeReturnToSelectStagePanel()
    {
        if (isReturnToSelectStagePanel)
        {
            //表示
            returnToSelectStagePanel.SetActive(true);
        }
        else
        {
            //非表示
            returnToSelectStagePanel.SetActive(false);
        }
    }

    /// <summary>
    /// タイトルへ戻るパネルの表示/非表示
    /// </summary>
    private void ChangeReturnToTitlePanel()
    {
        if (isReturnToTitlePanel)
        {
            //表示
            returnToTitlePanel.SetActive(true);
        }
        else
        {
            //非表示
            returnToTitlePanel.SetActive(false);
        }
    }
}
