using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using Unity.Profiling;


public class ComputeStat : MonoBehaviour
{
    
    // fps計測用の前回時刻
    public float updateInterval = 0.5F;
    private double lastInterval;
    private int frames;
    private float fps;

    // 画面で表示するUIパーツ情報
    public Text fpsValueText;
    public Text cpuFpsValueText;
    public Text gpuFpsValueText;
    public Text renderingValueText;
    public Text totalAllocatedValueText;

    // 履歴ダイアログ
    public GameObject logRootObject;

    // 画面で表示する文字パーツ情報
    public Text logText;

    private const int logLimitValue = 300;


    // プロファイラーからの各種使用量取得用
    Recorder behaviourUpdateRecorder;
    ProfilerRecorder renderingRecorder;

    // CPU処理時間
    long cpuNanoExec = 0;

    // GPU処理時間
    long gpuValue = 0;

    // Render Thread Time
    long renderThreadTime = 0;

    // メモリ使用量
    long totalAllocated = 0;

    long logResetLine = 0;

    // Start is called before the first frame update
    void Start()
    {
        frames = 0;
        behaviourUpdateRecorder = Recorder.Get("BehaviourUpdate");
        renderingRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Camera.Render");
        behaviourUpdateRecorder.enabled = true;
        logText.text = "FPS CPU時間 GPU時間 描画時間 メモリ使用量\r\n";
    }

    // Update is called once per frame
    void Update()
    {
        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + updateInterval)
        {
            fps = (float)(frames / (timeNow - lastInterval));
            frames = 0;
            lastInterval = timeNow;
        }
        fpsValueText.text = fps.ToString();
        setProfileParameter();
    }

    private void setProfileParameter()
    {
        // Cameraのレンダリング処理を計測用にCustomSamplerを作ります。
        var customSampler = CustomSampler.Create("MainCamera.Render", true);
        // そして、Recorderを取得してきます
        var customSampleRecorder = customSampler.GetRecorder();

        // Cameraのレンダリング処理全体をSampleします
        customSampler.Begin();
        Camera.main.Render();
        customSampler.End();

        // CPUとgpuの値を取得
        cpuNanoExec = behaviourUpdateRecorder.elapsedNanoseconds;
        gpuValue = customSampleRecorder.gpuElapsedNanoseconds;
        // render thread timeを取得
        renderThreadTime = renderingRecorder.LastValue;

        // メモリ使用量を取得
        totalAllocated = Profiler.GetTotalAllocatedMemoryLong();

        // 各パラメータを統計画面に反映
        cpuFpsValueText.text = cpuNanoExec + "ナノ秒";
        gpuFpsValueText.text = gpuValue + "ナノ秒";
        renderingValueText.text = renderThreadTime.ToString();
        totalAllocatedValueText.text = (totalAllocated/1024/1024).ToString() + "メガバイト";
        addLogLine();
    }

    private void addLogLine() {

        // 一定行を超えたら、リセットする
        if (logResetLine > logLimitValue) {
            logText.text = "FPS CPU時間 GPU時間 描画時間 メモリ使用量\r\n";
            logResetLine = 0;
        }
        logResetLine++;


        logText.text += fpsValueText.text + " , ";
        logText.text += cpuFpsValueText.text + " , ";
        logText.text += gpuFpsValueText.text + " , ";
        logText.text += renderingValueText.text + " , ";
        logText.text += totalAllocatedValueText.text;
        logText.text += "\r\n";
        print(logText.text);
    }

    public void toggleLog()
    {
        logRootObject.SetActive(!logRootObject.active);

    }

}


