using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class MeasureUIManager : MonoBehaviour
{
    [SerializeField] Sprite stopSprite, playSprite;
    [SerializeField] XROrigin xROrigin;
    ARPlaneManager aRPlaneManager;
    Sprite startSprite, pauseSprite;
    Button startStopButton, playPauseButton, retryButton;
    bool started, playing;
    void Start()
    {
        Button[] childrenButtons = GetComponentsInChildren<Button>();

        startStopButton = childrenButtons[0];
        playPauseButton = childrenButtons[1];
        retryButton = childrenButtons[2];

        startSprite = startStopButton.image.sprite;
        pauseSprite = playPauseButton.image.sprite;

        startStopButton.onClick.AddListener(HandleStartStop);
        playPauseButton.onClick.AddListener(HandlePlayPauseScan);
        retryButton.onClick.AddListener(HandleRetry);

        playPauseButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);

        aRPlaneManager = xROrigin.GetComponent<ARPlaneManager>();
        aRPlaneManager.enabled = false;
    }

    void HandleStartStop()
    {
        if (!started)
        {
            started = true;
            playing = true;
            aRPlaneManager.enabled = true;
            startStopButton.image.sprite = stopSprite;
            playPauseButton.gameObject.SetActive(true);
            retryButton.gameObject.SetActive(true);
            return;
        }
        started = false;
        playing = false;
        aRPlaneManager.enabled = false;
        startStopButton.image.sprite = startSprite;
        playPauseButton.image.sprite = pauseSprite;
        playPauseButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
    }

    void HandlePlayPauseScan()
    {
        if (!started) return;
        if (!playing)
        {
            playing = true;
            aRPlaneManager.enabled = true;
            playPauseButton.image.sprite = pauseSprite;
            return;
        }
        playing = false;
        aRPlaneManager.enabled = false;
        playPauseButton.image.sprite = playSprite;
    }

    void HandleRetry()
    {
        if (!started) return;
        started = false;
        playing = false;
        foreach (ARPlane plane in aRPlaneManager.trackables)
            plane.gameObject.SetActive(false);

        aRPlaneManager.enabled = false;
        startStopButton.image.sprite = startSprite;
        playPauseButton.image.sprite = pauseSprite;
        playPauseButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
    }
}
