using System;

namespace MagicLand_System_Web_Dev.Pages.DataContants
{
    public static class ClassData
    {
        public static readonly List<(string, string)> RoomOnlines, RoomOfflines, Slots;
        public static readonly List<(string, int)> DayOfWeeks;
        static ClassData()
        {
            RoomOnlines = new List<(string, string)>
            {
                ("GoogleMeet1", "21c2d354-1de5-4b67-950d-326ac832b4eb"),
                ("GoogleMeet2", "472d7b7a-22ce-4fcb-a436-3ea03fd29d78"),
                ("GoogleMeet3", "e1482984-8995-4fcc-8d5b-6014762814e8"),
            };

            RoomOfflines = new List<(string, string)>
            {
                ("LB1", "40514ccc-b66b-4093-af5f-0445210f9deb"),
                ("105", "db0c0da6-1a17-45a1-aeb6-091db19241fa"),
                ("104", "e6fc0f12-b135-4df4-bfe8-0ade3dcd2f1a"),
                ("210", "c2d5f218-2793-444c-a2d1-18fc81484c4f"),
                ("207", "c0434c38-eb33-4eae-98ff-2d5afc491330"),
                ("102", "f388a94d-d808-40bc-8fa8-ecebf64de0a4"),
                ("103", "cfddfd4d-f80b-4236-921c-599d812f1150"),
                ("101", "99f6f043-3fee-435f-a8ae-1f55f13b3256"),
                ("204", "be0c6afa-e24f-4132-aaa7-bb6408cab5a9"),
                ("LB2", "ea5b5f78-0223-4d7c-8950-4c7beb389e94")
            };

            Slots = new List<(string, string)>
            {
            ("417997AC-AFD7-4363-BFE5-6CDD56D4713A", "7:00 - 9:00"),
            ("301EFD4A-618E-4495-8E7E-DAA223D3945E", "9:15 - 11:15"),
            ("6AB50A00-08BA-483C-BF5D-0D55B05A2CCC", "12:00 - 14:00"),
            ("2291E53B-094B-493E-8132-C6494D2B18A8", "14:15 - 16:30"),
            ("688FE18C-5DB1-40AA-A7F3-F47CCD9FD395", "16:30 - 18:30"),
            ("418704FB-FAC8-4119-8795-C8FE5D348753", "19:00 - 21:00")
            };

            DayOfWeeks = new List<(string, int)> { ("monday", 1), ("tuesday", 2), ("wednesday", 3), ("thursday", 4), ("friday", 5), ("saturday", 6), ("sunday", 7) };
        }
    }
}
