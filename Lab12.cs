using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;

using MouseKeyboardLibrary;
using Timer_TCPServer;
using System.Threading;

namespace Lab12
{
    public partial class Lab12 : Form
    {
        private String log = String.Empty; // 로그 기록

        private Keys sleepKeys = Properties.Settings.Default.keyOfSleep; // 모니터 끄기 단축키
        private Keys shutdownKeys = Properties.Settings.Default.keyOfShutdown; // 컴퓨터 종료 단축키
        private Keys startKeys = Properties.Settings.Default.keyOfStart; // 컴퓨터 시작 단축키
        private Keys suspendKeys = Properties.Settings.Default.keyOfSuspend; // 대기 모드 단축키
        private Keys hibernateKeys = Properties.Settings.Default.keyOfHibernate; // 최대 절전 모드 단축키

        private int defaultCount = Properties.Settings.Default.Count; // 프로그램 실행 횟수
        private String id = Properties.Settings.Default.ID; // 등록된 ID

        MouseHook mouseHook = new MouseHook(); // mouseHook 생성
        KeyboardHook keyboardHook = new KeyboardHook(); // keyboardHook 생성

        Tester tester; // server의 매개변수
        TCPServer server;

        bool reserveFlag = false; // 전원 관리 예약 여부
        private string timerChoice; // 타이머 기능
        private int timerCount = 0; // 타이머 카운터
        private int timerMaxCount; // 타이머 입력 시간

        public Lab12()
        {
            /* 생성자 : Lab12()
             * 입력(매개변수) : 없음
             * 작업 : 컴포넌트 초기화와 이벤트 추가
             */
            InitializeComponent(); // 컴포넌트 초기화
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            mouseHook.MouseMove += new MouseEventHandler(mouseHook_MouseMove); // 마우스 이벤트 추가
            keyboardHook.KeyPress += new KeyPressEventHandler(keyboardHook_KeyPress); // 키보드 이벤트 추가

            button6.Text = sleepKeys.ToString(); // 모니터 끄기 단축키 표시
            button7.Text = shutdownKeys.ToString(); // 컴퓨터 종료 단축키 표시
            button8.Text = startKeys.ToString(); // 컴퓨터 켜기 단축키 표시
            button10.Text = suspendKeys.ToString(); // 대기 모드 단축키
            button12.Text = hibernateKeys.ToString(); // 최대 절전 모드 단축키

            this.MaximizeBox = false; // 최대화 버튼 비활성 화
        }

        private void turnOffMonitor()
        {
            /* 메소드 : private void turnOffMonitor()
             * 입력(매개변수) : 없음
             * 출력(반환값) : 없음
             * 작업 : 모니터가 꺼지며 서버에 sleep 로그를 기록하고 다시 가져와서 로그 출력
             */
            if (isEmptyID()) // ID 공백 여부 확인
                return;

            // log.asp에 접속하여 cmd=write 명령어로 sleep 로그 기록을 남김
            if ((getResponse("log.asp", "&cmd=write&action=sleep")) != "OK")
            {
                MessageBox.Show("Error occured!"); // 오류 발생 시 경고 발생
                return;
            }

            // log.asp에서 로그 기록을 가져옴
            log = getResponse("log.asp", "&cmd=read&action=sleep");
            printTextBox(); // 로그 기록을 로그 창(textBox2)에 출력

            // 마우스와 키보드 이벤트 감지 시작
            mouseHook.Start();
            keyboardHook.Start();

            // nircmd.exe 프로그램을 통해 모니터 끄기 수행
            Process.Start(fileName: "nircmd.exe", arguments: "monitor off");
        }

        private void shutdownComputer()
        {
            /* 메소드 : private void shutdownComputer()
             * 입력(매개변수) : 없음
             * 출력(반환값) : 없음
             * 작업 : 컴퓨터가 종료되며 서버에 shutdown 로그를 기록하고 다시 가져와서 로그 출력
             */
            if (isEmptyID()) // ID 공백 여부 확인
                return;

            // log.asp에 접속하여 cmd=write 명령어로 shutdown 로그 기록을 남김
            if ((getResponse("log.asp", "&cmd=write&action=shutdown")) != "OK")
            {
                MessageBox.Show("Error occured!");
                return;
            }

            // log.asp에서 로그 기록을 가져옴
            log = getResponse("log.asp", "&cmd=read&action=shutdown");
            printTextBox(); // 로그 기록을 로그 창(textBox2)에 출력

            // nircmd.exe 프로그램을 통해 컴퓨터 종료
            Process.Start(fileName: "nircmd.exe", arguments: "exitwin poweroff");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            /* 메소드 : protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
             * 입력(매개변수) : ref Message msg, Keys keyData
             * 출력(반환값) : bool 진릿값
             * 작업 : 단축키에 따른 기능 실행
             */

            // 모니터 끄기 단축키
            if (keyData.Equals(sleepKeys))
            {
                turnOffMonitor(); // 모니터 끄기
                return true;
            }
            else if (keyData.Equals(shutdownKeys)) // 컴퓨터 종료 단축키 입력 시
            {
                shutdownComputer(); // 컴퓨터 종료
                return true;
            }
            else if (keyData.Equals(startKeys)) // 컴퓨터 시작 단축키 입력 시
            {
                handleWakeup(); // 컴퓨터 시작
                return true;
            }
            else if (keyData.Equals(suspendKeys)) // 대기 모드 단축키 입력 시
            {
                suspendComputer(); // 대기 모드
                return true;
            }
            else if (keyData.Equals(hibernateKeys)) // 최대 절전 모드 단축키 입력 시
            {
                hibernateComputer(); // 최대 절전 모드
                return true;
            }

            return false;
        }

        private void handleWakeup()
        {
            /* 메소드 : private void handleWakeup()
             * 입력(매개변수) : 없음
             * 출력(반환값) : 없음
             * 작업 : 서버에 wakeup로그를 기록하고 다시 가져와서 출력한 다음 마우스, 키보드 이벤트 감지를 종료한다.
             */

            // log.asp에 접속하여 cmd=write 명령어로 wakeup 로그 기록을 남김
            if ((getResponse("log.asp", "&cmd=write&action=wakeup")) != "OK")
            {
                MessageBox.Show("Error occured!");
                return;
            }

            // log.asp에서 로그 기록을 가져옴
            log = getResponse("log.asp", "&cmd=read&action=wakeup");
            printTextBox(); // 로그 기록을 로그 창(textBox2)에 출력

            // 마우스, 키보드 이벤트 감지 종료
            mouseHook.Stop();
            keyboardHook.Stop();
        }

        private void mouseHook_MouseMove(object sender, MouseEventArgs e)
        {
            /* 메소드 : private void mouseHook_MouseMove(object sender, MouseEventArgs e)
             * 입력(매개변수) : object sender, MouseEventArgs e
             * 출력(반환값) : 없음
             * 작업 : handleWakeup()을 호출한다.
             */
            handleWakeup();
        }

        private void keyboardHook_KeyPress(object sender, KeyPressEventArgs e)
        {
            /* 메소드 : private void keyboardHook_KeyPress(object sender, KeyPressEventArgs e)
             * 입력(매개변수) : object sender, MouseEventArgs e
             * 출력(반환값) : 없음
             * 작업 : handleWakeup()을 호출한다.
             */
            handleWakeup();
        }

        private String getResponse(String restURL, String msg)
        {
            /* 메소드 : private String getResponse(String restURL, String msg)
             * 입력(매개변수) : String restURL, String msg
             * 출력(반환값) : String resultStr
             * 작업 : 매개변수 msg(메시지)를 서버로 보내고 서버로부터 되돌려 받은 값을 반환
             */

            // URL 정보
            String resultStr;
            String message = "id=" + id + msg; // 보낼 메시지
            String URL = "http://210.94.194.100:20151/" + restURL; // 서버 URL

            // 웹 통신 객체 생성, 헤더 정보 설정
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL); // URL로 HTTP 웹 서버요청 객체를 생성
            byte[] sendData = UTF8Encoding.UTF8.GetBytes(message); // message를 UTF8 byte[] 형으로 인코딩
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8"; // 콘텐트 타입 설정
            request.ContentLength = sendData.Length; // 콘텐트 길이 설정
            request.Method = "POST"; // POST방식 이용

            // 바디 내용 기록, 서버 전달
            StreamWriter sw = new StreamWriter(request.GetRequestStream());  // 요청 스트림 생성
            sw.Write(message); // message를 서버로 보냄
            sw.Close(); // 스트림 닫기

            // 서버에서 결과 회신, 통신 종료
            HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse(); // HTTP 웹 응답 객체를 생성
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8")); // 스트림 리더 생성(UTF-8로 인코딩)
            resultStr = streamReader.ReadToEnd(); // 스트림 리더로 읽은 데이터를 resultStr에 저장
            streamReader.Close(); // 스트림 닫기
            httpWebResponse.Close(); // 응답 객체 닫기

            return resultStr; // 결과 값 반환
        }

        private bool isEmptyID()
        {
            /* 메소드 : private bool isEmptyID()
             * 입력(매개변수) : 없음
             * 출력(반환값) : bool타입 진리값
             * 작업 : id(학번)가 입력되어 있는지 확인
             */

            // id 공백 여부 확인
            if (id == String.Empty)
            {
                MessageBox.Show("Please input userID"); // 경고 메시지 출력
                return true;
            }
            else
                return false;
        }

        private void printTextBox()
        {
            /* 메소드 : private void printTextBox()
             * 입력(매개변수) : 없음
             * 출력(반환값) : 없음
             * 작업 : textBox2(로그 창)에 로그 기록을 출력
             */

            // log 기록 중 <BR> 부분을 줄 바꿈 문자로 변경
            log = log.Replace("<BR>", Environment.NewLine);
            textBox2.Text = log; // 로그 출력
            // 로그 기록 창의 스크롤을 마지막 줄로 이동
            textBox2.SelectionStart = textBox2.Text.Length - 1;
            textBox2.ScrollToCaret();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button1_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : button1을 클릭하면 textBox1에 입력한 ID가 서버에 등록됨
             */

            string tempID = textBox1.Text.ToString(); // 입력한 ID 가져옴
            if (id != tempID)
                Properties.Settings.Default.Count = 0;
            id = tempID;

            String str = getResponse("registerUser.asp", null); // registerUser.asp을 통해 id가 등록됨

            // OK 반환 시
            if (str == "OK")
                MessageBox.Show("Register Complete");
            else if (str == "ERROR: Exist ID") // 존재하는 아이디인 경우
                MessageBox.Show("Existed ID");
            else // 그 외의 경우
                MessageBox.Show("Error");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button2_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : button2를 클릭하면 turnOffMonitor() 실행
             */
            turnOffMonitor(); // 모니터 끄기
        }

        private void button3_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button3_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : button3를 클릭하면 shutdownComputer() 실행
             */
            shutdownComputer(); // 컴퓨터 종료
        }

        private void button4_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button4_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : button4를 클릭하면 서버에 wakeup 로그를 기록하고 다시 가져와서 로그 출력
             */
            if (isEmptyID()) // ID 공백 여부 확인
                return;

            // log.asp에 접속하여 cmd=write 명령어로 wakeup 로그 기록을 남김
            if ((getResponse("log.asp", "&cmd=write&action=wakeup")) != "OK")
            {
                MessageBox.Show("Error occured!");
                return;
            }

            // log.asp에서 로그 기록을 가져옴
            log = getResponse("log.asp", "&cmd=read&action=wakeup");

            printTextBox(); // 로그 기록을 로그 창(textBox2)에 출력
        }

        private void button5_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button5_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : button5를 클릭하면 로그 기록을 지정한 파일에 출력
             */
            if (isEmptyID()) // ID 공백 여부 확인
                return;

            Stream stream; // 스트림 생성
            SaveFileDialog saveFileDialog = new SaveFileDialog(); // 저장 파일 다이얼로그 생성
            saveFileDialog.Filter = "txt files (*.txt)|*.txt"; // txt 파일로 필터링

            // 다이얼로그 창에서 저장할 파일 지정
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // 저장 파일 스트림 열기
                if ((stream = saveFileDialog.OpenFile()) != null)
                {
                    // 로그 기록 버퍼
                    byte[] buffer = Encoding.UTF8.GetBytes(log);
                    stream.Write(buffer, 0, buffer.Length); // 버퍼 내용을 스트림에 출력
                    stream.Close(); // 스트림 닫기
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button6_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : button6를 클릭하면 모니터 끄기 단축키 설정 창 활성화
             */

            if (isEmptyID()) // ID 공백 여부 확인
                return;

            HotkeyForm hotkeySleepForm = new HotkeyForm();
            hotkeySleepForm.setLabelText(sleepKeys.ToString()); // 현재 모니터 끄기 단축키 표시
            hotkeySleepForm.ShowDialog(); // 다이얼로그 창 열기
            
            // 단축키 입력 시
            if (hotkeySleepForm.getCheck())
            {
                sleepKeys = hotkeySleepForm.getKeys(); // 단축키 지정
                button6.Text = sleepKeys.ToString(); // 단축키 표시
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button7_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : button7을 클릭하면 컴퓨터 종료 단축키 설정 창 활성화
             */

            if (isEmptyID()) // ID 공백 여부 확인
                return;

            HotkeyForm hotkeyShutdownForm = new HotkeyForm();
            hotkeyShutdownForm.setLabelText(shutdownKeys.ToString()); // 현재 컴퓨터 종료 단축키 저장
            hotkeyShutdownForm.ShowDialog(); // 다이얼로그 창 열기

            // 단축키 입력 시
            if (hotkeyShutdownForm.getCheck())
            {
                shutdownKeys = hotkeyShutdownForm.getKeys(); // 단축키 지정
                button7.Text = shutdownKeys.ToString(); // 단축키 표시
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button6_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : button8를 클릭하면 컴퓨터 시작 단축키 설정 창 활성화
             */

            if (isEmptyID()) // ID 공백 여부 확인
                return;

            HotkeyForm hotkeyStartForm = new HotkeyForm();
            hotkeyStartForm.setLabelText(startKeys.ToString()); // 현재 컴퓨터 시작 단축키 저장
            hotkeyStartForm.ShowDialog(); // 다이얼로그 창 열기

            // 단축키 입력 시
            if (hotkeyStartForm.getCheck())
            {
                startKeys = hotkeyStartForm.getKeys(); // 단축키 지정
                button8.Text = startKeys.ToString(); // 단축키 표시
            }
        }

        private void Lab12_Load(object sender, EventArgs e)
        {
            /* 메소드 : private void Lab08_Load(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 폼 로드 시 ID 등록 여부와 컴퓨터 재시작 여부 확인
             */

            if (id != String.Empty) // id가 존재하는 경우
            {
                textBox1.Text = id; // textBox1에 id 표시
                if (defaultCount >= 2) // 프로그램 실행 횟수가 2회 이상
                {
                    // 폼 숨기기
                    this.Visible = false;
                    this.ShowInTaskbar = false;
                }
            }
            else
                return;

            // 서버로부터 로그 읽어오기
            String str = getResponse("log.asp", "&cmd=read&action=shutdown");

            // 마지막 줄 구하기
            str = str.Substring(0, str.Length - 4); // 마지막 <BR> 문자 제거
            int newLineIndex = str.LastIndexOf("<BR>"); // 마지막에서 두 번째 <BR> 문자 인덱스 검색
            String lastLine = str.Substring(newLineIndex + 4); // 마지막 줄

            // 마지막 명령어가 SHUTDOWN 이었는지 확인
            if (lastLine.IndexOf("SHUTDOWN") != -1)
            {
                // log.asp에 접속하여 cmd=write 명령어로 wakeup 로그 기록을 남김
                if ((getResponse("log.asp", "&cmd=write&action=wakeup")) != "OK")
                {
                    MessageBox.Show("Error occured!");
                    return;
                }

                // 마지막 종료 후 경과 시간 표시
                DateTime nowTime = DateTime.Now; // 현재 시간
                String strShutdownTime = lastLine.Substring(lastLine.LastIndexOf("|") + 1); // 컴퓨터가 종료된 시간 문자열
                DateTime shutdownTime = Convert.ToDateTime(strShutdownTime); // String을 DateTime으로 변환

                TimeSpan timeSpan = nowTime - shutdownTime; // 현재 시간과 컴퓨터가 종료된 시간 차이
                double diffTime = timeSpan.TotalMinutes; // 시간 차이를 분으로 표시

                MessageBox.Show("컴퓨터 종료 후 " + diffTime.ToString("N2") + "분 지났습니다."); // 경과 시간 메시지 박스로 표시

                // log.asp에서 로그 기록을 가져옴
                log = getResponse("log.asp", "&cmd=read&action=wakeup");
                printTextBox(); // 로그 출력
            }

        }

        private void Lab12_FormClosing(object sender, FormClosingEventArgs e)
        {
            /* 메소드 : private void Lab12_FormClosing(object sender, FormClosingEventArgs e)
             * 입력(매개변수) : object sender, FormClosingEventArgs e
             * 출력(반환값) : 없음
             * 작업 : 창 닫기 버튼을 눌렀을 때 윈폼 소멸하고 트레이 아이콘만 표시
             */
            e.Cancel = true; // 종료 방지
            this.Hide(); // 폼 숨기기
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void exitToolStripMenuItem_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 기본 설정 파일에 값 저장
             */

            // 수정된 값들을 기본 설정 파일에 저장
            Properties.Settings.Default.ID = id; // id 저장
            Properties.Settings.Default.Count++; // count 증가
            Properties.Settings.Default.keyOfSleep = sleepKeys; // 모니터 끄기 단축키 저장
            Properties.Settings.Default.keyOfShutdown = shutdownKeys; // 컴퓨터 종료 단축키 저장
            Properties.Settings.Default.keyOfStart = startKeys; // 컴퓨터 시작 단축키 저장
            Properties.Settings.Default.keyOfSuspend = suspendKeys; // 대기 모드
            Properties.Settings.Default.keyOfHibernate = hibernateKeys; // 최대 절전 모드
            Properties.Settings.Default.Save(); // 설정 저장

            Environment.Exit(0); // 프로그램 종료
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void showToolStripMenuItem_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 트레이 아이콘 컨텍스트 메뉴에서 Show를 누르면 폼 표시
             */
            this.Show();
        }

        private void sleepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void sleepToolStripMenuItem_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 트레이 아이콘 컨텍스트 메뉴에서 Sleep을 누르면 모니터 끄기
             */

            if (isEmptyID()) // ID 공백 여부 확인
                return;

            turnOffMonitor();
        }

        private void shutdownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void shutdownToolStripMenuItem_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 트레이 아이콘 컨텍스트 메뉴에서 Shutdown을 누르면 컴퓨터 종료
             */

            if (isEmptyID()) // ID 공백 여부 확인
                return;

            shutdownComputer();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void startToolStripMenuItem_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 트레이 아이콘 컨텍스트 메뉴에서 Start를 누르면 컴퓨터 시작
             */

            if (isEmptyID()) // ID 공백 여부 확인
                return;

            handleWakeup();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            /* 메소드 : private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
             * 입력(매개변수) : object sender, MouseEventArgs e
             * 출력(반환값) : 없음
             * 작업 : 트레이 아이콘 더블 클릭 시 윈폼 활성화
             */
            this.ShowInTaskbar = true; // 작업 표시줄에 표시
            this.Show(); // 윈폼 표시
        }

        private void Lab12_Resize(object sender, EventArgs e)
        {
            /* 메소드 : private void Lab12_Resize(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 윈폼 최소화 시 윈폼을 숨기고 트레이 아이콘 표시
             */
            // 최소화 되었을 시
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide(); // 숨김
            }
        }

        private void handleComputerOff()
        {
            /* 메소드 : private void handleComputerOff()
                * 입력(매개변수) : 없음
                * 출력(반환값) : 없음
                * 작업 : 종료 경고 창을 띄워 사용자의 확인을 받음
                */
            OffForm offForm = new OffForm(); // 종료 창 폼
            offForm.ShowDialog(); // 표시

            // 종료 취소를 원하는 경우
            if (offForm.getOffFlag())
                shutdownComputer(); // 컴퓨터 종료
        }

        private void suspendComputer()
        {
            /* 메소드 : private void suspendComputer()
             * 입력(매개변수) : 없음
             * 출력(반환값) : 없음
             * 작업 : 컴퓨터가 대기 모드로 바뀌며 서버에 suspend 로그를 기록하고 다시 가져와서 로그 출력
             */
            if (isEmptyID()) // ID 공백 여부 확인
                return;

            // log.asp에 접속하여 cmd=write 명령어로 suspend 로그 기록을 남김
            if ((getResponse("log.asp", "&cmd=write&action=suspend")) != "OK")
            {
                MessageBox.Show("Error occured!");
                return;
            }

            // log.asp에서 로그 기록을 가져옴
            log = getResponse("log.asp", "&cmd=read&action=suspend");
            printTextBox(); // 로그 기록을 로그 창(textBox2)에 출력

            // nircmd.exe 프로그램을 통해 대기 모드 실행
            Process.Start(fileName: "nircmd.exe", arguments: "standby force");
        }

        private void hibernateComputer()
        {
            /* 메소드 : private void hibernateComputer()
            * 입력(매개변수) : 없음
            * 출력(반환값) : 없음
            * 작업 : 컴퓨터가 최대 절전 모드로 바뀌며 서버에 hibernate 로그를 기록하고 다시 가져와서 로그 출력
            */
            if (isEmptyID()) // ID 공백 여부 확인
                return;

            // log.asp에 접속하여 cmd=write 명령어로 hibernate 로그 기록을 남김
            if ((getResponse("log.asp", "&cmd=write&action=hibernate")) != "OK")
            {
                MessageBox.Show("Error occured!");
                return;
            }

            // log.asp에서 로그 기록을 가져옴
            log = getResponse("log.asp", "&cmd=read&action=hibernate");
            printTextBox(); // 로그 기록을 로그 창(textBox2)에 출력

            // nircmd.exe 프로그램을 통해 최대 절전 모드 실행
            Process.Start(fileName: "rundll32.exe", arguments: "powrprof.dll, SetSuspendState");
        }

        private void handleMessage()
        {
            /* 메소드 : private void handleMessage()
             * 입력(매개변수) : 없음
             * 출력(반환값) : 없음
             * 작업 : 새로 받은 메시지를 분류하여 모니터를 끄거나 컴퓨터 종료
             */
            while (true)
            {
                while (tester.Message == null) { } // null 메시지 무시
                if (tester.Message == "sleep" || tester.Message == "SLEEP") // 모니터 끄기 메시지
                {
                    turnOffMonitor(); // 모니터 끄기
                    MessageBox.Show("모니터가 꺼집니다.");
                }
                else if (tester.Message == "off" || tester.Message == "OFF") // 컴퓨터 종료 메시지
                    handleComputerOff(); // 컴퓨터 종료 처리
                else if (tester.Message == "suspend" || tester.Message == "SUSPEND")
                    suspendComputer(); // 대기 모드 처리
                else if (tester.Message == "hibernate" || tester.Message == "HIBERNATE")
                    hibernateComputer(); // 최대 절전 모드 처리

                tester.Message = null; // 메시지 초기화
            }
        }

        private void runListenerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void runListenerToolStripMenuItem_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 소켓 리스너를 실행
             */
            if (isEmptyID()) // ID 공백 여부 확인
                return;

            // 서버 객체 생성
            tester = new Tester();
            server = new TCPServer(tester);
            server.Start(); // 서버 실행

            Thread msgThread = new Thread(new ThreadStart(handleMessage)); // 메시지 스레드 생성
            msgThread.Start(); // 스레드 시작
        }

        private void stopListenerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void stopListenerToolStripMenuItem_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 소켓 리스너를 중지
             */
            if (isEmptyID() || server == null) // ID 공백 여부, server 객체 생성 여부 확인
                return;

            server.stopListener(); // 리스너 중지
        }

        private void button9_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button9_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : button9를 클릭하면 서버에 대기 모드 로그를 기록하고 다시 가져와서 로그 출력
             */
            suspendComputer(); // 대기 모드 돌입
        }

        private void button11_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button11_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : button11를 클릭하면 서버에 최대 절전 모드 로그를 기록하고 다시 가져와서 로그 출력
             */
            hibernateComputer(); // 최대 절전 모드 돌입
        }

        private void button10_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button10_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : button10을 클릭하면 대기 모드 단축키 설정 창 활성화
             */
            if (isEmptyID()) // ID 공백 여부 확인
                return;

            HotkeyForm hotkeySuspendForm = new HotkeyForm();
            hotkeySuspendForm.setLabelText(suspendKeys.ToString()); // 현재 대기 모드 단축키 표시
            hotkeySuspendForm.ShowDialog(); // 다이얼로그 창 열기

            // 단축키 입력 시
            if (hotkeySuspendForm.getCheck())
            {
                suspendKeys = hotkeySuspendForm.getKeys(); // 단축키 지정
                button10.Text = suspendKeys.ToString(); // 단축키 표시
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button12_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : button12을 클릭하면 최대 절전 모드 단축키 설정 창 활성화
             */
            if (isEmptyID()) // ID 공백 여부 확인
                return;

            HotkeyForm hotkeyHibernateForm = new HotkeyForm();
            hotkeyHibernateForm.setLabelText(hibernateKeys.ToString()); // 현재 최대 절전 모드 단축키 표시
            hotkeyHibernateForm.ShowDialog(); // 다이얼로그 창 열기

            // 단축키 입력 시
            if (hotkeyHibernateForm.getCheck())
            {
                hibernateKeys = hotkeyHibernateForm.getKeys(); // 단축키 지정
                button12.Text = hibernateKeys.ToString(); // 단축키 표시
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button13_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 전원 관리 예약 등록 창 활성화
             */
            // 예약이 안되어 있는 경우
            if (!reserveFlag)
            {
                ReserveForm form = new ReserveForm(); // 예약 창
                form.ShowDialog(); // 창 표시

                if (form.getClickOkay()) // 예약 등록 시
                {
                    reserveFlag = true; 
                    button13.Enabled = false; // 예약 등록 버튼 비활성화
                    timerChoice = form.getChoice(); // 선택한 기능 가져오기
                    timerMaxCount = form.getCount(); // 타이머 시간 가져오기
                    timer1.Start(); // 타이머 시작
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            /* 메소드 : private void timer1_Tick(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 전원관리 예약 타이머 처리
             */
            label6.Text = (timerMaxCount - timerCount) + "초 후 " + timerChoice; // 타이머 시간 표시

            if (timerCount++ == timerMaxCount) // 타이머 시간 체크
            {
                switch (timerChoice) // 예약된 전원 관리 기능 실행
                {
                    case "sleep":
                        turnOffMonitor(); // 모니터 끄기
                        break;
                    case "shutdown":
                        shutdownComputer(); // 컴퓨터 종료
                        break;
                    case "suspend":
                        suspendComputer(); // 대기 모드
                        break;
                    case "hibernate":
                        hibernateComputer(); // 최대 절전 모드
                        break;
                }
                timer1.Stop(); // 타이머 종료
                button13.Enabled = true; // 타이머가 종료되면 예약 등록 버튼 활성화
                label6.Text = string.Empty;
                reserveFlag = false;
                timerCount = 0; // 타이머 카운트 초기화
            }

        }

        private void suspendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            suspendComputer();
        }

        private void hibernateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hibernateComputer();
        }

    }
}