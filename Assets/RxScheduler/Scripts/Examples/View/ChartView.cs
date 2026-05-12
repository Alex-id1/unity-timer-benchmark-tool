using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;

/// <summary>
/// Receives BenchmarkResult via MessageBroker and renders
/// each metrics series as a separate line/bar chart
/// </summary>
public class ChartView: MonoBehaviour {
    [SerializeField] private RectTransform _chartsGroupParentRT;
    private List<GameObject> chartsGOList = new List<GameObject>();

    private const string chartStr = "Chart";

    private ScreenshotCutter _screenshotCutter;


    private void Awake() {
        MessageBroker.Default.Receive<RxMsg>().Where(msg => msg.MsgType == RxMsgType.CREATE_CHART)
            .Subscribe(msg => OnCreateChartMsg(msg.Data))
            .AddTo(this);

        Init();
    }

    private void OnDestroy() {
        _screenshotCutter?.Dispose();
    }

    /// <summary>
    /// Captures the chart area and saves it as PNG at the given path
    /// </summary>
    public void SaveScreenshot(string path) {
        if (_screenshotCutter == null)
            _screenshotCutter = new ScreenshotCutter(_chartsGroupParentRT);

        StartCoroutine(_screenshotCutter.CaptureAndSave(path));
    }


    private void Init() {
        if(_chartsGroupParentRT == null) {
            Debug.LogError("Charts group parent object is not attached to ChartView");
            return;
        }

        var vertLayoutGroup = _chartsGroupParentRT.GetComponent<VerticalLayoutGroup>();
        if(vertLayoutGroup == null)
            vertLayoutGroup = _chartsGroupParentRT.gameObject.AddComponent<VerticalLayoutGroup>();

        vertLayoutGroup.childAlignment = TextAnchor.UpperLeft;
        vertLayoutGroup.reverseArrangement = false;
        vertLayoutGroup.childControlHeight = vertLayoutGroup.childControlWidth = true;
        vertLayoutGroup.childScaleHeight = vertLayoutGroup.childScaleWidth = false;
        vertLayoutGroup.childForceExpandHeight = vertLayoutGroup.childForceExpandWidth = true;

    }

    // -----------------------------------------
    //  Message handling
    // -----------------------------------------
    private void OnCreateChartMsg(object benchmarkResult) {
        Reset();

        if(benchmarkResult == null || benchmarkResult.GetType() != typeof(BenchmarkResult)) {
            Debug.LogError("[OnCreateChartMsg] benchmarkResult is null or its type is wrong");
            return;
        }
        var result = (BenchmarkResult)benchmarkResult;

        if(result == null) {
            Debug.LogError("[OnCreateChartMsg] benchmarkResult is null");
            return;
        }

        if(result.Config.IncludeFps)
            CreateChart(ChartType.LINE, "FPS", result.Fps);
        if(result.Config.IncludeGc)
            CreateChart(ChartType.LINE, "GC", result.Gc);
        if(result.Config.IncludeCpuTime)
            CreateChart(ChartType.BAR, "CPU Time", result.CpuTime);       
    }

    
    // -----------------------------------------
    //  Chart population
    // -----------------------------------------
    private void CreateChart(ChartType chartType, string chartName, List<double> data) {
        GameObject chartGO = new GameObject(string.Concat(chartName, chartStr), typeof(RectTransform));
        chartGO.transform.SetParent(_chartsGroupParentRT, false);
        chartsGOList.Add(chartGO);

        Serie serie;
        BaseChart chart;
        if(chartType is ChartType.LINE) {
            chart = chartGO.AddComponent<LineChart>();
            chart.Init();
            chart.RemoveData();
            serie = chart.AddSerie<Line>(chartName);
        } else {
            chart = chartGO.AddComponent<SimplifiedBarChart>();
            chart.Init();
            chart.RemoveData();
            serie = chart.AddSerie<SimplifiedBar>(chartName);
            serie.clip = true;
        }

        chart.GetChartComponent<Title>().text = chartName;
        chart.EnsureChartComponent<Tooltip>().show = true;
        chart.EnsureChartComponent<Legend>().show = true;

        var yAxis = chart.GetChartComponent<YAxis>();
        yAxis.minMaxType = Axis.AxisMinMaxType.MinMax;
        if(chartType == ChartType.LINE) {
            double ceilRate = CalculateCeilRate(data);
            yAxis.ceilRate = ceilRate;
            int decimals = ceilRate >= 1 ? 0 : Mathf.CeilToInt((float)-Math.Log10(ceilRate));
            yAxis.axisLabel.numericFormatter = decimals > 0 ? $"F{decimals}" : "F0";
        }

        for(int i = 0; i < data.Count; i++) {
            chart.AddData(chartName, data[i]);
        }
    }

    private double CalculateCeilRate(List<double> data) {
        if(data == null || data.Count == 0) return 1;
        double min = double.MaxValue;
        double max = double.MinValue;
        foreach(var v in data) { if(v < min) min = v; if(v > max) max = v; }
        double range = max - min;
        if(range <= 0) return 0.1;
        double rate = Math.Pow(10, Math.Floor(Math.Log10(range)));
        return Math.Max(rate, 0.1);
    }

    private void Reset() {
        for(int i = 0; i < chartsGOList.Count; i++) {
            if(chartsGOList[i] != null)
                Destroy(chartsGOList[i]);
        }
        chartsGOList.Clear();
    }
}