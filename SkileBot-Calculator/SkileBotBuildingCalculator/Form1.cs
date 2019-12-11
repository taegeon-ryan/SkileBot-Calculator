using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkileBotBuildingCalculator
{
    public partial class Form1 : Form
    {
        #region 데이터 모델
        // 그린건설, 러스관광 주가, 요일
        int green_price = 1000;
        int rus_price = 1000;
        Day day = Day.Weekday;

        enum Day
        {
            Weekday,
            Saturday,
            Sunday
        }

        enum Building
        {
            White,
            House,
            ConvenienceStore,
            School,
            Company,
            Hospital,
            Bank,
            DepartmentStore,
            Hotel,
            Casino,
            Port,
            Stadium,
            Church,
            Factory
        }

        enum BuildPrice
        {
            White = 0,
            House = 100,
            ConvenienceStore = 250,
            School = 700,
            Company = 1500,
            Hospital = 3200,
            Bank = 7000,
            DepartmentStore = 15000,
            Hotel = 30000,
            Casino = 50000,
            Port = 25000,
            Stadium = 25000,
            Church = 20000,
            Factory = 1000
        }

        enum Price
        {
            White = 0,
            House = 1,
            ConvenienceStore = 2,
            School = 4,
            Company = 6,
            Hospital = 10,
            Bank = 20,
            DepartmentStore = 25,
            Hotel = 30,
            Casino = 50,
            Port = 15,
            Stadium = 15,
            Church = 0,
            Factory = 0
        }

        double[] BuildPeriod = {
            0,
            1,
            2,
            3,
            4,
            5,
            5.5,
            6,
            7,
            10,
            7,
            7,
            6,
            3
        };

        string[] Effect =
        {
            "양 옆 x0.5",
            "없음",
            "2칸 이내 주택/학교 x4.0, 3칸 이내 편의점 x0.7",
            "4칸 이내 주택 x3.0, 2칸 이내 호텔 x0.5",
            "3칸 이내 학교/회사/카지노 제외 모든 건물 x2.0",
            "모든 건물 x1.5",
            "양 옆 회사 x5.0",
            "양 옆 모든 건물 x5.0, 전체 범위 편의점 x0.5",
            "전체 범위 주택/학교/호텔/카지노 제외 모든 건물 x2.0, 전체 범위 주택 x0.5",
            "2칸 이내 주택 x0.5, 전체 범위 카지노 x0.5, 양 옆 호텔 x2.0, 러스관광 주가 영향",
            "전체 범위 호텔/카지노/경기장 x3.0, A, G구역에만 건설 가능",
            "2칸 이내 모든 건물 x0.2, 토요일에 경기장 x2.0",
            "2칸 이내 주택/회사 x1.5, 일요일에 주택/회사 x5.0로 강화",
            "3칸 이내 모든 건물 x0.0, 모든 건물 건설속도 3배(중복X)"
        };
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Initialize();

            // 포인터 속성 변경
            this.label15.Cursor = Cursors.Hand;
            this.label20.Cursor = Cursors.Hand;
        }

        #region 콤보 박스 초기화
        private void Initialize()
        {
            List<ComboBox> cbItems = new List<ComboBox> { cbAItem, cbBItem, cbCItem, cbDItem, cbEItem, cbFItem, cbGItem };
            string[] building_names = { "공터", "주택", "편의점", "학교", "회사", "병원", "은행", "백화점", "호텔", "카지노", "항구", "경기장", "교회", "공장" };
            foreach (var cbItem in cbItems)
            {
                cbItem.Items.AddRange(building_names);
                if (cbItem.Name != "cbAItem" && cbItem.Name != "cbGItem") // A, G 구역이 아니면
                {
                    cbItem.Items.Remove("항구");
                }
                cbItem.SelectedItem = "공터";
            }
            List<TextBox> priceNow = new List<TextBox> { tbAPriceNow, tbBPriceNow, tbCPriceNow, tbDPriceNow, tbEPriceNow, tbFPriceNow, tbGPriceNow };
        }
        #endregion

        #region 콤보 박스 변동을 감지
        private void cbAItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            WhenComboBoxItemChanges(cbAItem);
        }

        private void cbBItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            WhenComboBoxItemChanges(cbBItem);
        }

        private void cbCItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            WhenComboBoxItemChanges(cbCItem);
        }

        private void cbDItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            WhenComboBoxItemChanges(cbDItem);
        }

        private void cbEItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            WhenComboBoxItemChanges(cbEItem);
        }

        private void cbFItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            WhenComboBoxItemChanges(cbFItem);
        }

        private void cbGItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            WhenComboBoxItemChanges(cbGItem);
        }
        #endregion

        // 콤보 박스가 변동하면?
        private void WhenComboBoxItemChanges(ComboBox cbItem)
        {
            var pbItem = GetPictureBoxName(cbItem);
            ChangePictureBoxImages(cbItem, pbItem);
            var lblItems = GetLabelName(cbItem);
            ChangeLabels(cbItem, lblItems);
            ChangePriceNow();
            ChangeBuildPrice();
        }

        #region 선택한 건물에 해당하는 사진으로 바꾸기
        private PictureBox GetPictureBoxName(ComboBox cbItem)
        {
            string cbName = cbItem.Name;
            string pbName = "pb";

            if (cbName.Contains("A")) {
                pbName += "A";
            }
            else if (cbName.Contains("B"))
            {
                pbName += "B";
            }
            else if (cbName.Contains("C"))
            {
                pbName += "C";
            }
            else if (cbName.Contains("D"))
            {
                pbName += "D";
            }
            else if (cbName.Contains("E"))
            {
                pbName += "E";
            }
            else if (cbName.Contains("F"))
            {
                pbName += "F";
            }
            else if (cbName.Contains("G"))
            {
                pbName += "G";
            }

            PictureBox pb = Controls.Find(pbName, true).First() as PictureBox;
            return pb;
        }

        private void ChangePictureBoxImages(ComboBox cbItem, PictureBox pbItem)
        {
            switch (cbItem.SelectedItem.ToString())
            {
                case "공터":
                    pbItem.Image = Properties.Resources.white;
                    break;
                case "주택":
                    pbItem.Image = Properties.Resources.house;
                    break;
                case "편의점":
                    pbItem.Image = Properties.Resources.convenience_store;
                    break;
                case "학교":
                    pbItem.Image = Properties.Resources.school;
                    break;
                case "회사":
                    pbItem.Image = Properties.Resources.office;
                    break;
                case "병원":
                    pbItem.Image = Properties.Resources.hospital;
                    break;
                case "은행":
                    pbItem.Image = Properties.Resources.bank;
                    break;
                case "백화점":
                    pbItem.Image = Properties.Resources.department_store;
                    break;
                case "호텔":
                    pbItem.Image = Properties.Resources.hotel;
                    break;
                case "카지노":
                    pbItem.Image = Properties.Resources.mosque;
                    break;
                case "항구":
                    pbItem.Image = Properties.Resources.ship;
                    break;
                case "경기장":
                    pbItem.Image = Properties.Resources.stadium;
                    break;
                case "교회":
                    pbItem.Image = Properties.Resources.church;
                    break;
                case "공장":
                    pbItem.Image = Properties.Resources.factory;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 선택한 건물에 해당하는 데이터(건설비용, 집세, 건설기간, 효과)로 바꾸기
        private List<Label> GetLabelName(ComboBox cbItem)
        {
            string cbName = cbItem.Name;
            string lblName = "lbl";

            if (cbName.Contains("A"))
            {
                lblName += "A";
            }
            else if (cbName.Contains("B"))
            {
                lblName += "B";
            }
            else if (cbName.Contains("C"))
            {
                lblName += "C";
            }
            else if (cbName.Contains("D"))
            {
                lblName += "D";
            }
            else if (cbName.Contains("E"))
            {
                lblName += "E";
            }
            else if (cbName.Contains("F"))
            {
                lblName += "F";
            }
            else if (cbName.Contains("G"))
            {
                lblName += "G";
            }

            Label lblBuildPrice = Controls.Find(lblName + "BuildPrice", true).First() as Label;
            Label lblPrice = Controls.Find(lblName + "Price", true).First() as Label;
            Label lblBuildPeriod = Controls.Find(lblName + "BuildPeriod", true).First() as Label;
            Label lblEffect = Controls.Find(lblName + "Effect", true).First() as Label;

            List<Label> lblItems = new List<Label> { lblBuildPrice, lblPrice, lblBuildPeriod, lblEffect };
            return lblItems;
        }

        private List<Label> GetLabelName(ComboBox cbItem, int n)
        {
            string cbName = cbItem.Name;
            string lblName = "lbl";

            if (cbName.Contains("A"))
            {
                lblName += "A";
            }
            else if (cbName.Contains("B"))
            {
                lblName += "B";
            }
            else if (cbName.Contains("C"))
            {
                lblName += "C";
            }
            else if (cbName.Contains("D"))
            {
                lblName += "D";
            }
            else if (cbName.Contains("E"))
            {
                lblName += "E";
            }
            else if (cbName.Contains("F"))
            {
                lblName += "F";
            }
            else if (cbName.Contains("G"))
            {
                lblName += "G";
            }

            Label lblBuildPrice = Controls.Find(lblName + "BuildPrice", true).First() as Label;
            Label lblPrice = Controls.Find(lblName + "Price", true).First() as Label;

            List<Label> lblItems = new List<Label> { lblBuildPrice, lblPrice };
            return lblItems;
        }

        private void ChangeLabels(ComboBox cbItem, List<Label> lblItems)
        {
            switch (cbItem.SelectedItem.ToString())
            {
                case "공터":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.White * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.White * green_price);
                    lblItems[2].Text = $"{BuildPeriod[(int)Building.White]}일";
                    lblItems[3].Text = $"{Effect[(int)Building.White]}";
                    break;
                case "주택":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.House * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.House * green_price);
                    lblItems[2].Text = $"{BuildPeriod[(int)Building.House]}일";
                    lblItems[3].Text = $"{Effect[(int)Building.House]}";
                    break;
                case "편의점":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.ConvenienceStore * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.ConvenienceStore * green_price);
                    lblItems[2].Text = $"{BuildPeriod[(int)Building.ConvenienceStore]}일";
                    lblItems[3].Text = $"{Effect[(int)Building.ConvenienceStore]}";
                    break;
                case "학교":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.School * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.School * green_price);
                    lblItems[2].Text = $"{BuildPeriod[(int)Building.School]}일";
                    lblItems[3].Text = $"{Effect[(int)Building.School]}";
                    break;
                case "회사":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Company * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Company * green_price);
                    lblItems[2].Text = $"{BuildPeriod[(int)Building.Company]}일";
                    lblItems[3].Text = $"{Effect[(int)Building.Company]}";
                    break;
                case "병원":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Hospital * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Hospital * green_price);
                    lblItems[2].Text = $"{BuildPeriod[(int)Building.Hospital]}일";
                    lblItems[3].Text = $"{Effect[(int)Building.Hospital]}";
                    break;
                case "은행":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Bank * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Bank * green_price);
                    lblItems[2].Text = $"{BuildPeriod[(int)Building.Bank]}일";
                    lblItems[3].Text = $"{Effect[(int)Building.Bank]}";
                    break;
                case "백화점":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.DepartmentStore * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.DepartmentStore * green_price);
                    lblItems[2].Text = $"{BuildPeriod[(int)Building.DepartmentStore]}일";
                    lblItems[3].Text = $"{Effect[(int)Building.DepartmentStore]}";
                    break;
                case "호텔":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Hotel * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Hotel * green_price);
                    lblItems[2].Text = $"{BuildPeriod[(int)Building.Hotel]}일";
                    lblItems[3].Text = $"{Effect[(int)Building.Hotel]}";
                    break;
                case "카지노":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Casino * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Casino * rus_price);
                    lblItems[2].Text = $"{BuildPeriod[(int)Building.Casino]}일";
                    lblItems[3].Text = $"{Effect[(int)Building.Casino]}";
                    break;
                case "항구":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Port * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Port * green_price);
                    lblItems[2].Text = $"{BuildPeriod[(int)Building.Port]}일";
                    lblItems[3].Text = $"{Effect[(int)Building.Port]}";
                    break;
                case "경기장":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Stadium * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Stadium * green_price);
                    lblItems[2].Text = $"{BuildPeriod[(int)Building.Stadium]}일";
                    lblItems[3].Text = $"{Effect[(int)Building.Stadium]}";
                    break;
                case "교회":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Church * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Church * green_price);
                    lblItems[2].Text = $"{BuildPeriod[(int)Building.Church]}일";
                    lblItems[3].Text = $"{Effect[(int)Building.Church]}";
                    break;
                case "공장":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Factory * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Factory * green_price);
                    lblItems[2].Text = $"{BuildPeriod[(int)Building.Factory]}일";
                    lblItems[3].Text = $"{Effect[(int)Building.Factory]}";
                    break;
                default:
                    break;
            }
        }

        private void ChangeLabels(ComboBox cbItem, List<Label> lblItems, int n)
        {
            switch (cbItem.SelectedItem.ToString())
            {
                case "공터":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.White * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.White * green_price);
                    break;
                case "주택":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.House * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.House * green_price);
                    break;
                case "편의점":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.ConvenienceStore * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.ConvenienceStore * green_price);
                    break;
                case "학교":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.School * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.School * green_price);
                    break;
                case "회사":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Company * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Company * green_price);
                    break;
                case "병원":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Hospital * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Hospital * green_price);
                    break;
                case "은행":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Bank * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Bank * green_price);
                    break;
                case "백화점":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.DepartmentStore * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.DepartmentStore * green_price);
                    break;
                case "호텔":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Hotel * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Hotel * green_price);
                    break;
                case "카지노":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Casino * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Casino * rus_price);
                    break;
                case "항구":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Port * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Port * green_price);
                    break;
                case "경기장":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Stadium * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Stadium * green_price);
                    break;
                case "교회":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Church * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Church * green_price);
                    break;
                case "공장":
                    lblItems[0].Text = string.Format("{0:#,##0}슷", (int)BuildPrice.Factory * green_price);
                    lblItems[1].Text = string.Format("{0:#,##0}슷", (int)Price.Factory * green_price);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 현재 집세와 집세 총계, 건설비용 총합 계산하여 바꾸기
        private void ChangePriceNow()
        {
            double[] now_prices = new double[7] { 0, 0, 0, 0, 0, 0, 0 };
            Building[] buildingTypes = new Building[14] { Building.White, Building.White, Building.White, Building.White, Building.White, Building.White, Building.White,
            Building.White, Building.White, Building.White, Building.White, Building.White, Building.White, Building.White };
            List<ComboBox> cbItems = new List<ComboBox> { cbAItem, cbBItem, cbCItem, cbDItem, cbEItem, cbFItem, cbGItem };
            List<TextBox> tbItems = new List<TextBox> { tbAPriceNow, tbBPriceNow, tbCPriceNow, tbDPriceNow, tbEPriceNow, tbFPriceNow, tbGPriceNow };
            try
            {
                for (int i = 0; i < cbItems.Count; i++)
                {
                    switch (cbItems[i].SelectedItem.ToString())
                    {
                        case "공터":
                            buildingTypes[i] = Building.White;
                            break;
                        case "주택":
                            buildingTypes[i] = Building.House;
                            break;
                        case "편의점":
                            buildingTypes[i] = Building.ConvenienceStore;
                            break;
                        case "학교":
                            buildingTypes[i] = Building.School;
                            break;
                        case "회사":
                            buildingTypes[i] = Building.Company;
                            break;
                        case "병원":
                            buildingTypes[i] = Building.Hospital;
                            break;
                        case "은행":
                            buildingTypes[i] = Building.Bank;
                            break;
                        case "백화점":
                            buildingTypes[i] = Building.DepartmentStore;
                            break;
                        case "호텔":
                            buildingTypes[i] = Building.Hotel;
                            break;
                        case "카지노":
                            buildingTypes[i] = Building.Casino;
                            break;
                        case "항구":
                            buildingTypes[i] = Building.Port;
                            break;
                        case "경기장":
                            buildingTypes[i] = Building.Stadium;
                            break;
                        case "교회":
                            buildingTypes[i] = Building.Church;
                            break;
                        case "공장":
                            buildingTypes[i] = Building.Factory;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception) { return; }

            for (int i = 0; i < cbItems.Count; i++)
            {
                now_prices[i] = Calculate(i, buildingTypes);
            }

            for (int i = 0; i < cbItems.Count; i++)
            {
                tbItems[i].Text = string.Format("{0:#,##0}슷", now_prices[i]);
            }

            int totalPrice = 0;
            foreach (var item in tbItems)
            {
                int price = int.Parse(item.Text.Replace("슷", "").Replace(",", ""));
                totalPrice += price;
            }
            lblPriceNowTotal.Text = string.Format("{0:#,##0}", totalPrice);
        }

        private double Calculate(int index, Building[] types)
        {
            Building current_type = types[index];
            double price = 0;

            #region 초기값 설정
            switch (current_type) {
                case Building.White:
                    price = (int)Price.White * green_price;
                    break;
                case Building.House:
                    price = (int)Price.House * green_price;
                    break;
                case Building.ConvenienceStore:
                    price = (int)Price.ConvenienceStore * green_price;
                    break;
                case Building.School:
                    price = (int)Price.School * green_price;
                    break;
                case Building.Company:
                    price = (int)Price.Company * green_price;
                    break;
                case Building.Hospital:
                    price = (int)Price.Hospital * green_price;
                    break;
                case Building.Bank:
                    price = (int)Price.Bank * green_price;
                    break;
                case Building.DepartmentStore:
                    price = (int)Price.DepartmentStore * green_price;
                    break;
                case Building.Hotel:
                    price = (int)Price.Hotel * green_price;
                    break;
                case Building.Casino:
                    price = (int)Price.Casino * rus_price;
                    break;
                case Building.Port:
                    price = (int)Price.Port * green_price;
                    break;
                case Building.Stadium:
                    price = (int)Price.Stadium * green_price;
                    break;
                case Building.Church:
                    price = (int)Price.Church * green_price;
                    break;
                case Building.Factory:
                    price = (int)Price.Factory * green_price;
                    break;
            }
            #endregion

            int f = 0;
            int e = 0;

            #region 공터 : 양옆 집세 50%
            f = index - 1;
            e = index + 1;

            if (f < 0) f = 0;
            if (e > types.Length - 1) e = types.Length - 1;

            for (; f <= e; f++)
            {
                if (f == index)
                {
                    continue;
                }
                if (types[f] == Building.White)
                {
                    price *= 0.5;
                }
            }
            #endregion

            #region 편의점 : 2칸 이내 주택/학교 집세 x4.0, 3칸 이내 편의점 집세 x0.7
            f = index - 2;
            e = index + 2;

            if (f < 0) f = 0;
            if (e > types.Length - 1) e = types.Length - 1;

            for (; f <= e; f++)
            {
                if (f == index)
                {
                    continue;
                }
                if (types[f] == Building.ConvenienceStore)
                {
                    if (current_type == Building.House || current_type == Building.School)
                    {
                        price *= 4;
                    }
                }
            }

            f = index - 3;
            e = index + 3;

            if (f < 0) f = 0;
            if (e > types.Length - 1) e = types.Length - 1;

            for (; f <= e; f++)
            {
                if (f == index)
                {
                    continue;
                }
                if (types[f] == Building.ConvenienceStore)
                {
                    if (current_type == Building.ConvenienceStore)
                    {
                        price *= 0.7;
                    }
                }
            }
            #endregion

            #region 학교 : 4칸 이내 주택 집세 x3.0, 2칸 이내 호텔 집세 x0.5
            f = index - 4;
            e = index + 4;

            if (f < 0) f = 0;
            if (e > types.Length - 1) e = types.Length - 1;

            for (; f <= e; f++)
            {
                if (f == index)
                {
                    continue;
                }
                if (types[f] == Building.School)
                {
                    if (current_type == Building.House)
                    {
                        price *= 3;
                    }
                }
            }

            f = index - 2;
            e = index + 2;

            if (f < 0) f = 0;
            if (e > types.Length - 1) e = types.Length - 1;

            for (; f <= e; f++)
            {
                if (f == index)
                {
                    continue;
                }
                if (types[f] == Building.School)
                {
                    if (current_type == Building.Hotel)
                    {
                        price *= 0.5;
                    }
                }
            }
            #endregion

            #region 회사 - 3칸 이내 학교/회사/카지노를 제외한 모든 건물 x2.0
            f = index - 3;
            e = index + 3;

            if (f < 0) f = 0;
            if (e > types.Length - 1) e = types.Length - 1;

            for (; f <= e; f++)
            {
                if (f == index)
                {
                    continue;
                }
                if (types[f] == Building.Company)
                {
                    if (current_type != Building.School && current_type != Building.Company && current_type != Building.Casino)
                    {
                        price *= 2.0;
                    }
                }
            }
            #endregion

            #region 병원 - 전체 범위 내 모든 건물 집세 x1.5
            for (int i = 0; i < types.Length; i++)
            {
                if (i == index)
                {
                    continue;
                }
                if (types[i] == Building.Hospital)
                {
                    price *= 1.5;
                }
            }
            #endregion

            #region 은행 - 양 옆 회사 집세 x5.0
            f = index - 1;
            e = index + 1;

            if (f < 0) f = 0;
            if (e > types.Length - 1) e = types.Length - 1;

            for (; f <= e; f++)
            {
                if (f == index)
                {
                    continue;
                }
                if (types[f] == Building.Bank)
                {
                    if (current_type == Building.Company)
                    {
                        price *= 5;
                    }
                }
            }
            #endregion

            #region 백화점 - 양 옆 모든 건물 집세 x3.5, 전체 범위 내 편의점 집세 x0
            f = index - 1;
            e = index + 1;

            if (f < 0) f = 0;
            if (e > types.Length - 1) e = types.Length - 1;

            for (; f <= e; f++)
            {
                if (f == index)
                {
                    continue;
                }
                if (types[f] == Building.DepartmentStore)
                {
                    price *= 3.5;
                }
            }

            for (int i = 0; i < 7; i++)
            {
                if (i == index)
                {
                    continue;
                }
                if (types[i] == Building.DepartmentStore)
                {
                    if (current_type == Building.ConvenienceStore)
                    {
                        price *= 0;
                    }
                }
            }
            #endregion

            #region 호텔 - 전체 범위 내 주택/학교/호텔/카지노를 제외한 모든 건물 집세 x2.0, 전체 범위 내 주택 집세 x0.5
            for (int i = 0; i < 7; i++)
            {
                if (i == index)
                {
                    continue;
                }
                if (types[i] == Building.Hotel)
                {
                    if (current_type != Building.House && current_type != Building.School && current_type != Building.Hotel && current_type != Building.Casino)
                    {
                        price *= 2;
                    }
                }
            }

            for (int i = 0; i < 7; i++)
            {
                if (i == index)
                {
                    continue;
                }
                if (types[i] == Building.Hotel)
                {
                    if (current_type == Building.House)
                    {
                        price *= 0.5;
                    }
                }
            }
            #endregion

            #region 카지노 - 2칸 이내 주택 집세 x0.5, 전체 범위 내 카지노 집세 x0.5, 양 옆 호텔 집세 x2.0
            f = index - 2;
            e = index + 2;

            if (f < 0) f = 0;
            if (e > types.Length - 1) e = types.Length - 1;

            for (; f <= e; f++)
            {
                if (f == index)
                {
                    continue;
                }
                if (types[f] == Building.Casino)
                {
                    if (current_type == Building.House)
                    {
                        price *= 0.5;
                    }
                }
            }

            for (int i = 0; i < 7; i++)
            {
                if (i == index)
                {
                    continue;
                }
                if (types[i] == Building.Casino)
                {
                    if (current_type == Building.Casino)
                    {
                        price *= 0.5;
                    }
                }
            }

            f = index - 1;
            e = index + 1;

            if (f < 0) f = 0;
            if (e > types.Length - 1) e = types.Length - 1;

            for (; f <= e; f++)
            {
                if (f == index)
                {
                    continue;
                }
                if (types[f] == Building.Casino)
                {
                    if (current_type == Building.Hotel)
                    {
                        price *= 2;
                    }
                }
            }
            #endregion

            #region 항구 - 전체 범위 내 호텔/카지노/경기장 집세 x3.0
            for (int i = 0; i < 7; i++)
            {
                if (i == index)
                {
                    continue;
                }
                if (types[i] == Building.Port)
                {
                    if (current_type == Building.Hotel || current_type == Building.Casino || current_type == Building.Stadium)
                    {
                        price *= 3;
                    }
                }
            }
            #endregion

            #region 경기장 - 2칸 이내 모든 건물 집세 x0.2, 토요일에 경기장 집세 x4.0
            f = index - 2;
            e = index + 2;

            if (f < 0) f = 0;
            if (e > types.Length - 1) e = types.Length - 1;

            for (; f <= e; f++)
            {
                if (f == index)
                {
                    continue;
                }
                if (types[f] == Building.Stadium)
                {
                    price *= 0.2;
                }
            }

            if (day == Day.Saturday)
            {
                if (types[index] == Building.Stadium)
                {
                    price *= 4;
                }
            }
            #endregion

            #region 교회 - 2칸 이내 주택/회사 집세 x1.5, 일요일에 주택/회사에 주는 집세 효과 x5.0로 강화
            f = index - 2;
            e = index + 2;

            if (f < 0) f = 0;
            if (e > types.Length - 1) e = types.Length - 1;

            for (; f <= e; f++)
            {
                if (f == index)
                {
                    continue;
                }
                if (types[f] == Building.Church)
                {
                    if (current_type == Building.House || current_type == Building.Company)
                    {
                        if (day == Day.Sunday)
                        {
                            price *= 5;
                        }
                        else
                        {
                            price *= 1.5;
                        }
                    }
                }
            }
            #endregion

            #region 공장 - 3칸 이내 모든 건물 집세 x0
            f = index - 3;
            e = index + 3;

            if (f < 0) f = 0;
            if (e > types.Length - 1) e = types.Length - 1;

            for (; f <= e; f++)
            {
                if (f == index)
                {
                    continue;
                }
                if (types[f] == Building.Factory)
                {
                    price *= 0;
                }
            }
            #endregion

            return price;
        }

        private void ChangeBuildPrice()
        {
            int totalBuildPrice = 0;
            List<ComboBox> cbItems = new List<ComboBox> { cbAItem, cbBItem, cbCItem, cbDItem, cbEItem, cbFItem, cbGItem };
            Building[] buildingTypes = new Building[14] { Building.White, Building.White, Building.White, Building.White, Building.White, Building.White, Building.White
                , Building.White, Building.White, Building.White, Building.White, Building.White, Building.White, Building.White};
            
            try
            {
                for (int i = 0; i < cbItems.Count; i++)
                {
                    switch (cbItems[i].SelectedItem.ToString())
                    {
                        case "공터":
                            buildingTypes[i] = Building.White;
                            break;
                        case "주택":
                            buildingTypes[i] = Building.House;
                            break;
                        case "편의점":
                            buildingTypes[i] = Building.ConvenienceStore;
                            break;
                        case "학교":
                            buildingTypes[i] = Building.School;
                            break;
                        case "회사":
                            buildingTypes[i] = Building.Company;
                            break;
                        case "병원":
                            buildingTypes[i] = Building.Hospital;
                            break;
                        case "은행":
                            buildingTypes[i] = Building.Bank;
                            break;
                        case "백화점":
                            buildingTypes[i] = Building.DepartmentStore;
                            break;
                        case "호텔":
                            buildingTypes[i] = Building.Hotel;
                            break;
                        case "카지노":
                            buildingTypes[i] = Building.Casino;
                            break;
                        case "항구":
                            buildingTypes[i] = Building.Port;
                            break;
                        case "경기장":
                            buildingTypes[i] = Building.Stadium;
                            break;
                        case "교회":
                            buildingTypes[i] = Building.Church;
                            break;
                        case "공장":
                            buildingTypes[i] = Building.Factory;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception) { return; }

            for (int i = 0; i < cbItems.Count; i++)
            {
                switch (buildingTypes[i])
                {
                    case Building.White:
                        totalBuildPrice += (int)BuildPrice.White;
                        break;
                    case Building.House:
                        totalBuildPrice += (int)BuildPrice.House;
                        break;
                    case Building.ConvenienceStore:
                        totalBuildPrice += (int)BuildPrice.ConvenienceStore;
                        break;
                    case Building.School:
                        totalBuildPrice += (int)BuildPrice.School;
                        break;
                    case Building.Company:
                        totalBuildPrice += (int)BuildPrice.Company;
                        break;
                    case Building.Hospital:
                        totalBuildPrice += (int)BuildPrice.Hospital;
                        break;
                    case Building.Bank:
                        totalBuildPrice += (int)BuildPrice.Bank;
                        break;
                    case Building.DepartmentStore:
                        totalBuildPrice += (int)BuildPrice.DepartmentStore;
                        break;
                    case Building.Hotel:
                        totalBuildPrice += (int)BuildPrice.Hotel;
                        break;
                    case Building.Casino:
                        totalBuildPrice += (int)BuildPrice.Casino;
                        break;
                    case Building.Port:
                        totalBuildPrice += (int)BuildPrice.Port;
                        break;
                    case Building.Stadium:
                        totalBuildPrice += (int)BuildPrice.Stadium;
                        break;
                    case Building.Church:
                        totalBuildPrice += (int)BuildPrice.Church;
                        break;
                    case Building.Factory:
                        totalBuildPrice += (int)BuildPrice.Factory;
                        break;
                    default:
                        break;
                }
            }

            tbTotalBuildPrice.Text = string.Format("{0:#,##0}", totalBuildPrice * green_price);
        }
        #endregion

        // 그린건설 주가 적용 버튼을 누르면?
        private void btnApplyGreenPrice_Click(object sender, EventArgs e)
        {
            int temp = green_price;
            ComboBox[] cbItems = new ComboBox[] { cbAItem, cbBItem, cbCItem, cbDItem, cbEItem, cbFItem, cbGItem };
            try
            {
                green_price = int.Parse(tbGreenPrice.Text);
                foreach (var item in cbItems)
                {
                    var lblItems = GetLabelName(item, 2);
                    ChangeLabels(item, lblItems, 2);
                }
                ChangePriceNow();
                ChangeBuildPrice();
            }
            catch (Exception)
            {
                MessageBox.Show("잘못된 입력값", "입력값이 잘못되었습니다.");
                green_price = temp;
            }
        }

        // 러스관광 주가 적용 버튼을 누르면?
        private void btnRusPriceApply_Click(object sender, EventArgs e)
        {
            int temp = rus_price;
            ComboBox[] cbItems = new ComboBox[] { cbAItem, cbBItem, cbCItem, cbDItem, cbEItem, cbFItem, cbGItem };
            try
            {
                rus_price = int.Parse(tbRusPrice.Text);
                foreach (var item in cbItems)
                {
                    var lblItems = GetLabelName(item, 2);
                    ChangeLabels(item, lblItems, 2);
                }
                ChangePriceNow();
                ChangeBuildPrice();
            }
            catch (Exception)
            {
                MessageBox.Show("잘못된 입력값", "입력값이 잘못되었습니다.");
                rus_price = temp;
            }
        }

        private void chbSat_CheckedChanged(object sender, EventArgs e)
        {
            if (chbSat.Checked)
            {
                chbSun.Checked = false;
                day = Day.Saturday;
            } 
            else
            {
                day = Day.Weekday;
            }
            ChangePriceNow();
            ChangeBuildPrice();
        }

        private void chbSun_CheckedChanged(object sender, EventArgs e)
        {
            if (chbSun.Checked)
            {
                chbSat.Checked = false;
                day = Day.Sunday;
            }
            else
            {
                day = Day.Weekday;
            }
            ChangePriceNow();
            ChangeBuildPrice();
        }

        private void label20_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/taegeon-ryan/SkileBot-Calculator");
        }

        private void label15_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://team-crescendo.me");
        }
    }
}
