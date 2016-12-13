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
    public partial class HotkeyForm : Form
    {
        private Keys keys; // 입력한 단축키
        private bool check = false; // 확인 버튼 클릭 여부

        public HotkeyForm()
        {
            /* 생성자 : HotkeyForm()
             * 입력(매개변수) : 없음
             * 작업 : 컴포넌트 초기화
             */
            InitializeComponent();
        }

        public Keys getKeys()
        {
            /* 메소드 : public Keys getKeys()
             * 입력(매개변수) : 없음
             * 출력(반환값) : Keys keys
             * 작업 : 단축키 반환
             */
            return keys;
        }

        public bool getCheck()
        {
            /* 메소드 : public bool getCheck()
             * 입력(매개변수) : 없음
             * 출력(반환값) : bool 진릿값
             * 작업 : 확인 버튼 클릭 여부
             */
            return check;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            /* 메소드 : protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
             * 입력(매개변수) : ref Message msg, Keys keyData
             * 출력(반환값) : bool 진릿값
             * 작업 : 단축키를 누르면 label2에 표시
             */

            keys = keyData; // 필드 keys에 keyData 저장
            label2.Text = keys.ToString(); // 입력한 키를 라벨에 표시
            label2.TextAlign = ContentAlignment.MiddleCenter; // 가운데 정렬

            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button1_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 확인 버튼 클릭 시 check를 true로 변환
             */
            check = true;
            Close(); // 창 닫기
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /* 메소드 : private void button2_Click(object sender, EventArgs e)
             * 입력(매개변수) : object sender, EventArgs e
             * 출력(반환값) : 없음
             * 작업 : 취소 버튼 클릭 시 check를 false로 변환
             */
            check = false;
            Close(); // 창 닫기
        }

        public void setLabelText(string text)
        {
            /* 메소드 : public void setLabelText(string text)
             * 입력(매개변수) : string texet
             * 출력(반환값) : 없음
             * 작업 : label2에 text 표시
             */
            label2.Text = text; // 라벨에 text 표시
        }

    }
}
