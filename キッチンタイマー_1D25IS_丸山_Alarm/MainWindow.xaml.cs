using System;
// ファイルやフォルダーを扱う機能、音声ファイルのパスを作るために使う。
using System.IO;

// 音声を再生する機能を使うため
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace キッチンタイマー_1D25IS_丸山_Alarm
{
    public partial class MainWindow : Window
    {
        // カウントダウン用タイマー
        private DispatcherTimer timer = new DispatcherTimer();

        // アラーム音を再生するための変数
        private SoundPlayer alarmPlayer;

        // 残り時間（秒）
        private int remainingSeconds = 0;

        // 数字ボタンで入力した文字列
        private string inputText = "";

        // アラームが鳴っているか
        private bool isAlarmPlaying = false;

        // タイマーを開始したことがあるか
        private bool hasStarted = false;

        public MainWindow()
        {
            InitializeComponent();

            // カウントダウンの設定
            // タイマーのが1秒経過するたびにTimer_Tickを呼び出す
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            // アラーム音ファイルの読み込み
            string soundPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Sounds",
                "alarm_light.wav"
            );

            // 音声ファイルを読み込む
            alarmPlayer = new SoundPlayer(soundPath);
            alarmPlayer.Load();

            // 初期画面の表示
            UpdateTimeDisplay();

            btnStop.IsEnabled = false;
            btnAlarmStop.IsEnabled = false;
        }

        // 数字ボタンを押したとき
        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            // タイマー開始後、またはアラーム中なら数字を入力できないようにしている
            if (hasStarted || isAlarmPlaying)
                return;

            // 押されたボタンを取得
            Button button = (Button)sender;

            // ボタンを文字列型に変換して変数に代入
            string number = button.Content.ToString();

            // 最大4桁まで入力可能、入力文字数の確認
            if (inputText.Length >= 4)
                return;

            // 数字を追加する
            inputText += number;

            // 時間を計算して画面を更新する
            UpdateTimeFromInput();
        }

        // 1桁削除
        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            // タイマー開始後、またはアラーム中なら数字を入力できないようにしている
            if (hasStarted || isAlarmPlaying)
                return;

            // 削除する文字が存在するときだけ、次の処理に進む
            if (inputText.Length > 0)
            {
                // Substring() は、文字列の一部分を取り出すメソッド
                // 文字列.Substring(開始位置, 取り出す文字数)
                // 先頭から2文字を取り出す
                inputText = inputText.Substring(0, inputText.Length - 1);
            }

            UpdateTimeFromInput();
        }

        // 全消去
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (hasStarted || isAlarmPlaying)
                return;

            // 保存されている文字列を空にする
            inputText = "";

            // 残り時間を0にする
            remainingSeconds = 0;

            UpdateTimeDisplay();
        }

        // スタート
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // 時間が0秒ならタイマーを開始しない
            if (remainingSeconds <= 0)
                return;

            // アラーム中は開始しない
            if (isAlarmPlaying)
                return;

            // タイマーが開始したことを記録する
            hasStarted = true;

            // カウントダウン開始
            timer.Start();

            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
        }

        // 1秒ごとに実行される処理
        // 残り時間を1秒減らす処理
        private void Timer_Tick(object sender, EventArgs e)
        {
            // 残り時間が0秒より大きい場合だけ、カウントダウン処理を実行する
            if (remainingSeconds > 0)
            {
                // 残り時間を1秒減らす
                remainingSeconds--;

                // 時間表示を更新
                UpdateTimeDisplay();

                // 0秒になったらアラームを鳴らす
                if (remainingSeconds == 0)
                {
                    // カウントダウン用のタイマーを停止する
                    // 残り時間そのものを0にするわけではない
                    // だから再開すると、続きから続けられる
                    timer.Stop();

                    btnStart.IsEnabled = false;
                    btnStop.IsEnabled = false;

                    // アラームを開始する
                    StartAlarm();
                }
            }
        }

        // ストップ（一時停止）
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            // カウントダウンを停止する
            timer.Stop();

            // 残り時間が0秒より大きいかどうかを判定して、その結果を IsEnabled に設定
            btnStart.IsEnabled = remainingSeconds > 0;
            btnStop.IsEnabled = false;
        }

        // リセット
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            // カウントダウンを停止
            timer.Stop();

            // アラームも停止
            StopAlarm();

            // 残り時間と入力内容と開始状態を初期化
            remainingSeconds = 0;
            inputText = "";
            hasStarted = false;

            // 画面表示を初期化
            UpdateTimeDisplay();

            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        // アラーム開始
        private void StartAlarm()
        {
            // アラームが鳴っている状態にする
            isAlarmPlaying = true;

            btnStart.IsEnabled = false;
            btnStop.IsEnabled = false;

            // アラーム停止ボタンを有効にする
            btnAlarmStop.IsEnabled = true;

            // 音声ファイルを繰り返し再生
            alarmPlayer.PlayLooping();
        }

        // アラーム停止ボタン
        private void btnAlarmStop_Click(object sender, RoutedEventArgs e)
        {
            // アラーム停止
            StopAlarm();

            // 残り時間と入力内容と開始状態を初期化
            remainingSeconds = 0;
            inputText = "";
            hasStarted = false;

            UpdateTimeDisplay();

            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        // アラーム停止処理
        private void StopAlarm()
        {
            // alarmPlayer が null ではない場合だけ、処理を実行する
            // 音声ファイルの読み込みに失敗して、変数に何も設定されていない場合に備えている
            if (alarmPlayer != null)
            {
                // 音声を停止する
                alarmPlayer.Stop();
            }

            // アラームが鳴っていない状態に戻す
            isAlarmPlaying = false;
            btnAlarmStop.IsEnabled = false;
        }

        // 入力した数字を時間に変換
        private void UpdateTimeFromInput()
        {
            int value = 0;

            // 入力文字列がnullまたは空文字列ではないか確認する
            if (!string.IsNullOrEmpty(inputText))
            {
                // 文字列を数値に変換
                int.TryParse(inputText, out value);
            }

            // 分を計算をする
            int minutes = value / 100;

            // 秒の計算をする
            int seconds = value % 100;

            // 合計秒数に変換
            remainingSeconds = minutes * 60 + seconds;

            UpdateTimeDisplay();
        }

        // 残り時間を画面に表示
        private void UpdateTimeDisplay()
        {
            // 残り時間から分を計算する
            int minutes = remainingSeconds / 60;

            // 残り時間から秒を計算する
            int seconds = remainingSeconds % 60;

            // 画面に表示
            // D2は2桁で表示したい時に使用する
            txtTime.Text = $"{minutes:D2}:{seconds:D2}";
        }

        // ウィンドウを閉じるとき
        // protected：このクラスや、継承したクラスから利用できるアクセス修飾子
        // override：親クラスにあるメソッドを上書きする
        //OnClosed：ウィンドウが閉じられたときに呼び出されるメソッド。
        protected override void OnClosed(EventArgs e)
        {
            // カウントダウン用タイマーを停止する
            timer.Stop();

            // 音声再生用の変数が存在するか確認する
            if (alarmPlayer != null)
            {
                // アラーム音を停止する
                alarmPlayer.Stop();

                // 使い終わった音声再生用のオブジェクトを片づける
                alarmPlayer.Dispose();
            }

            // 親クラスの終了処理を呼び出す
            base.OnClosed(e);
        }
    }
}
