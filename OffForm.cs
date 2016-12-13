using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace Lab12
{
    public partial class OffForm : Form
    {
        private bool offFlag = false; // 종료 체크 플래그
        private Timer timer; // 타이머
        private int timerCount = 1; // 타이머 카운트

        public bool getOffFlag()
        {
            /* 메소드 : public bool getOffFlag()
             * 입력(매개변수) : 없음
             * 출력(반환값) : bool offFlag
             * 작업 : offFlag(종료 체크 플래그) 여부를 반환한다.
             */
            return offFlag;
        }

        public OffForm()
        {
            /* 생성자 : public OffForm()
             * 입력(매개변수) : 없음
             * 작업 : 컴포넌트와 타이머를 초기화한다.
             */
            InitializeComponent();

            // 타이머 이용
            timer = new Timer();
            timer.Interval = 1000; // 1000msec = 1sec
            timer.Tick += new EventHandler(timerTick); // 타이머 이벤트를 추가한다.
            timer.Start(); // 타이머 시작
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button1_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 바로 종료
             */
            offFlag = true; // 바로 종료
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button2_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 종료 취소
             */
            offFlag = false; // 종료 취소
            this.Close();
        }

        void timerTick(object sender, EventArgs e)
        {
            /* 메소드 : void timer_Tick(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 타이머 처리
             */
            // 프로그레스 바를 한 단계 진전
            progressBar1.PerformStep();
            
            if (timerCount++ == 15) // 15초 지나면
            {
                timer.Stop(); // 타이머 종료
                offFlag = true; // 종료 처리
                this.Close();
            }
        }
    
    }
}
