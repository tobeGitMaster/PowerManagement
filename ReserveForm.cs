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
    public partial class ReserveForm : Form
    {
        private bool clickOkay = false; // 예약 등록 여부
        private string choice = "sleep"; // 기본 설정은 모니터 끄기
        private int count = 0; // 카운트 수 0으로 초기화

        public ReserveForm()
        {
            InitializeComponent();
        }

        public bool getClickOkay()
        {
            /* 메소드 : public bool getClickOkay()
             * 입력(매개변수) : 없음
             * 출력(반환값) : bool clickOkay
             * 작업 : 등록 버튼 클릭 여부 반환
             */
            return clickOkay;
        }

        public string getChoice()
        {
            /* 메소드 : public string getChoice()
             * 입력(매개변수) : 없음
             * 출력(반환값) : string choice
             * 작업 : 선택한 전원 관리 기능 반환
             */
            return choice;    
        }

        public int getCount()
        {
            /* 메소드 : public string getCount()
             * 입력(매개변수) : 없음
             * 출력(반환값) : int count
             * 작업 : 입력한 타이머 시간 반환
             */
            return count;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            /* 메소드 : private void radioButton1_CheckedChanged(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 모니터 끄기 버튼을 체크한 경우 choice값을 "sleep"으로 변경
             */
            choice = "sleep";
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            /* 메소드 : private void radioButton2_CheckedChanged(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 컴퓨터 종료 버튼을 체크한 경우 choice값을 "shutdown"으로 변경
             */
            choice = "shutdown";
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            /* 메소드 : private void radioButton3_CheckedChanged(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 대기 모드 버튼을 체크한 경우 choice값을 "suspend"로 변경
             */
            choice = "suspend";
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            /* 메소드 : private void radioButton4_CheckedChanged(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 최대 절전 모드 버튼을 체크한 경우 choice값을 "hibernate"로 변경
             */
            choice = "hibernate";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button1_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 등록 버튼을 클릭한 경우 등록 여부와 타이머 시간 반환
             */
            if (textBox1.Text != "") // 시간 입력을 한 경우
            {
                clickOkay = true; // 등록 버튼 
                count = int.Parse(textBox1.Text); // 입력한 시간 값 가져오기
            }
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button1_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 취소 버튼을 클릭한 경우 예약 창 닫기
             */
            this.Close();
        }

    }
}
