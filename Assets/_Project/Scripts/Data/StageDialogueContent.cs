using ProjectKai.Data;

namespace ProjectKai.Data
{
    /// <summary>
    /// 모든 스테이지의 대화 데이터를 코드로 제공.
    /// 돈키호테 모티프: "풍차에 돌진하는 바보가 세상을 바꾼다"
    /// </summary>
    public static class StageDialogueContent
    {
        // ═══════════════════════════════════════
        // 1장: "풍차를 향해"
        // ═══════════════════════════════════════

        public static DialogueLine[] Stage1_1_Intro => new[]
        {
            new DialogueLine { speakerName = "리나", text = "카이, 간단한 일이야. 폐허 던전에 고블린이 늘었대. 청소 의뢰." },
            new DialogueLine { speakerName = "카이", text = "보수는?" },
            new DialogueLine { speakerName = "리나", text = "2만 골드. 쉬운 돈이잖아." },
            new DialogueLine { speakerName = "카이", text = "슬럼 아이들이 위험하다고?" },
            new DialogueLine { speakerName = "리나", text = "...그건 의뢰에 없어." },
            new DialogueLine { speakerName = "카이", text = "(검을 잡으며) 됐어. 가자." },
        };

        public static DialogueLine[] Stage1_1_Clear => new[]
        {
            new DialogueLine { speakerName = "카이", text = "이 정도는 준비운동이지." },
            new DialogueLine { speakerName = "리나", text = "아직 안쪽이 남았어. 지하로 내려가." },
        };

        public static DialogueLine[] Stage1_2_Intro => new[]
        {
            new DialogueLine { speakerName = "카이", text = "지하인가... 공기부터 다르군." },
            new DialogueLine { speakerName = "리나", text = "조심해. 지하 고블린은 더 강할 수 있어." },
        };

        public static DialogueLine[] Stage1_2_Clear => new[]
        {
            new DialogueLine { speakerName = "카이", text = "이건... 에테르 수정? 누가 이런 걸 고블린한테?" },
            new DialogueLine { speakerName = "리나", text = "그 수정, 시장에 안 도는 군용이야. 누군가 큰 걸 준비하고 있어." },
            new DialogueLine { speakerName = "카이", text = "의뢰 내용 바뀌었다." },
            new DialogueLine { speakerName = "리나", text = "보수 안 올라가는데?" },
            new DialogueLine { speakerName = "카이", text = "알아." },
        };

        public static DialogueLine[] Stage1_3_Intro => new[]
        {
            new DialogueLine { speakerName = "", text = "\"풍차인가, 거인인가\"" },
            new DialogueLine { speakerName = "카이", text = "...큰 놈이 있군." },
            new DialogueLine { speakerName = "리나", text = "에테르 반응이 비정상적이야. 조심해, 카이." },
        };

        public static DialogueLine[] Stage1_3_Clear => new[]
        {
            new DialogueLine { speakerName = "카이", text = "이 문양... 그림자 기사단?" },
            new DialogueLine { speakerName = "리나", text = "카이, 이거 단순한 의뢰가 아니었어. 이 던전, 그림자 기사단의 실험장이야." },
            new DialogueLine { speakerName = "카이", text = "...보수 올려." },
            new DialogueLine { speakerName = "리나", text = "이건 용병이 건드릴 수준이 아니야. 빠지자." },
            new DialogueLine { speakerName = "카이", text = "못 빠져. 이 아이들 누가 지켜?" },
            new DialogueLine { speakerName = "리나", text = "...또 시작이야, 돈키호테 선생님." },
            new DialogueLine { speakerName = "카이", text = "풍차가 아니면 좋겠지만." },
        };

        // ═══════════════════════════════════════
        // 2장: "거울의 기사"
        // ═══════════════════════════════════════

        public static DialogueLine[] Stage2_1_Intro => new[]
        {
            new DialogueLine { speakerName = "리나", text = "그림자 기사단의 전초 기지야. 경비가 빡빡할 거야." },
            new DialogueLine { speakerName = "카이", text = "기사단이라... 그들도 한때는 기사도를 믿었을 텐데." },
            new DialogueLine { speakerName = "리나", text = "지금은 아니니까 조심해." },
        };

        public static DialogueLine[] Stage2_1_Clear => new[]
        {
            new DialogueLine { speakerName = "카이", text = "여기 병사들... 전부 에테르에 취해 있어." },
            new DialogueLine { speakerName = "리나", text = "인체 실험이야. 더 안쪽으로 가야 해." },
        };

        public static DialogueLine[] Stage2_2_Intro => new[]
        {
            new DialogueLine { speakerName = "리나", text = "에테르 농도가 계속 올라가고 있어. 오래 있으면 안 돼." },
            new DialogueLine { speakerName = "카이", text = "빨리 끝내지." },
        };

        public static DialogueLine[] Stage2_2_Clear => new[]
        {
            new DialogueLine { speakerName = "카이", text = "이 앞이... 기사단장인가." },
            new DialogueLine { speakerName = "리나", text = "돌아와, 카이. 이건 네 수준이 아니야." },
            new DialogueLine { speakerName = "카이", text = "알아. 그래도 가야 해." },
        };

        public static DialogueLine[] Stage2_3_Intro => new[]
        {
            new DialogueLine { speakerName = "", text = "\"거울 앞에 선 기사\"" },
            new DialogueLine { speakerName = "기사단장", text = "또 한 명의 바보가 왔군. 기사도를 믿는 건가?" },
            new DialogueLine { speakerName = "카이", text = "믿는 게 아니라 지키는 거야." },
            new DialogueLine { speakerName = "기사단장", text = "그래? 그럼 보여줘라, 네 기사도가 뭘 할 수 있는지." },
        };

        public static DialogueLine[] Stage2_3_Defeat => new[]
        {
            new DialogueLine { speakerName = "기사단장", text = "기사도로 세상을 바꿀 수 있었나?" },
            new DialogueLine { speakerName = "카이", text = "...아직이야." },
            new DialogueLine { speakerName = "기사단장", text = "돌아와라. 더 강해진 뒤에. 그때 답을 보여줘." },
            new DialogueLine { speakerName = "리나", text = "카이! 빼내야 해! ...빨리!" },
        };

        // ═══════════════════════════════════════
        // 3장: "라만차의 기사"
        // ═══════════════════════════════════════

        public static DialogueLine[] Stage3_1_Intro => new[]
        {
            new DialogueLine { speakerName = "카이", text = "이번엔 다르다." },
            new DialogueLine { speakerName = "리나", text = "...강해졌네. 눈빛이 달라." },
            new DialogueLine { speakerName = "카이", text = "져봐야 알 수 있는 것들이 있어." },
        };

        public static DialogueLine[] Stage3_1_Clear => new[]
        {
            new DialogueLine { speakerName = "카이", text = "점점 기사단 본진에 가까워지고 있어." },
            new DialogueLine { speakerName = "리나", text = "여기서부터는 정예야. 방심하지 마." },
        };

        public static DialogueLine[] Stage3_2_Intro => new[]
        {
            new DialogueLine { speakerName = "리나", text = "이 앞이 마지막 관문이야." },
            new DialogueLine { speakerName = "카이", text = "리나. 고마워. 여기까지 와줘서." },
            new DialogueLine { speakerName = "리나", text = "...아직 고마워할 때가 아니야, 바보." },
        };

        public static DialogueLine[] Stage3_2_Clear => new[]
        {
            new DialogueLine { speakerName = "카이", text = "기사단장. 이번엔 내가 간다." },
        };

        public static DialogueLine[] Stage3_3_Intro => new[]
        {
            new DialogueLine { speakerName = "", text = "\"라만차의 기사, 다시 일어서다\"" },
            new DialogueLine { speakerName = "기사단장", text = "다시 왔나, 바보 기사." },
            new DialogueLine { speakerName = "카이", text = "라만차의 기사는 원래 바보야." },
            new DialogueLine { speakerName = "기사단장", text = "...재밌군. 이번엔 진심으로 상대해 주지." },
        };

        public static DialogueLine[] Stage3_3_Clear => new[]
        {
            new DialogueLine { speakerName = "기사단장", text = "...인정하지. 네 기사도는 진짜였다." },
            new DialogueLine { speakerName = "카이", text = "기사도가 세상을 바꾸냐고 물었지." },
            new DialogueLine { speakerName = "기사단장", text = "..." },
            new DialogueLine { speakerName = "카이", text = "바꾸더라. 조금씩, 천천히." },
            new DialogueLine { speakerName = "리나", text = "...드디어 끝났어." },
            new DialogueLine { speakerName = "카이", text = "아니. 그림자 기사단은... 시작에 불과했어." },
            new DialogueLine { speakerName = "", text = "\"풍차에 돌진하는 바보가 세상을 조금씩 바꾼다.\"" },
        };

        // ═══════════════════════════════════════
        // 거점 (Hub) — 리나 랜덤 대사
        // ═══════════════════════════════════════

        public static string[][] HubRinaDialogues => new[]
        {
            new[] { "리나", "오늘도 의뢰가 밀렸어. 빨리 골라." },
            new[] { "리나", "그 검 좀 닦아. 녹슬면 아무도 못 벤다." },
            new[] { "리나", "카이, 밥은 먹었어? 기사도로 배 채울 순 없잖아." },
            new[] { "리나", "...너, 진짜 기사도 같은 거 믿는 거야?" },
            new[] { "리나", "의뢰비 아직 안 들어왔어. 좀만 기다려." },
            new[] { "리나", "슬럼 아이들이 너 이야기 하더라. '기사 아저씨'래." },
            new[] { "리나", "오늘 날씨가 좋네. 네온 불빛 빼면." },
            new[] { "리나", "그림자 기사단... 신서울 어디에나 있대. 조심해." },
            new[] { "리나", "가끔 생각해. 네가 없었으면 나도 여기 없었을 거야." },
            new[] { "리나", "돈키호테 선생님, 오늘은 어느 풍차를 상대할 거야?" },
            new[] { "리나", "에테르 가격이 또 올랐대. 이 동네 살기 힘들어지겠어." },
            new[] { "리나", "...나도 가끔은 믿고 싶어. 네가 말하는 기사도." },
        };

        /// <summary>
        /// 스테이지 이름으로 도입 대사 반환
        /// </summary>
        public static DialogueLine[] GetIntroDialogue(string stageName)
        {
            return stageName switch
            {
                "1-1" => Stage1_1_Intro,
                "1-2" => Stage1_2_Intro,
                "1-3" => Stage1_3_Intro,
                "2-1" => Stage2_1_Intro,
                "2-2" => Stage2_2_Intro,
                "2-3" => Stage2_3_Intro,
                "3-1" => Stage3_1_Intro,
                "3-2" => Stage3_2_Intro,
                "3-3" => Stage3_3_Intro,
                _ => null,
            };
        }

        /// <summary>
        /// 스테이지 이름으로 클리어 대사 반환
        /// </summary>
        public static DialogueLine[] GetClearDialogue(string stageName)
        {
            return stageName switch
            {
                "1-1" => Stage1_1_Clear,
                "1-2" => Stage1_2_Clear,
                "1-3" => Stage1_3_Clear,
                "2-1" => Stage2_1_Clear,
                "2-2" => Stage2_2_Clear,
                "2-3" => Stage2_3_Defeat, // 패배 연출
                "3-1" => Stage3_1_Clear,
                "3-2" => Stage3_2_Clear,
                "3-3" => Stage3_3_Clear,
                _ => null,
            };
        }

        /// <summary>
        /// 스테이지 이름으로 다음 씬 이름 반환
        /// </summary>
        public static string GetNextScene(string stageName)
        {
            return stageName switch
            {
                "1-1" => "Stage1_2",
                "1-2" => "Stage1_3_Boss",
                "1-3" => "Hub",
                "2-1" => "Stage2_2",
                "2-2" => "Stage2_3_Boss",
                "2-3" => "Hub",
                "3-1" => "Stage3_2",
                "3-2" => "Stage3_3_Boss",
                "3-3" => "Credits",
                _ => "Hub",
            };
        }
    }
}
