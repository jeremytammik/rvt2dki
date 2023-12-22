namespace rvt2dki
{
    class DkiGeo
    {
        // Huellflaeche;Huellflaechentyp;Fenstertyp;TWD-Typ;Diskretisierung;Wandart;Hypokauste;Abschattung;Wandflaeche(m²);Fensterflaeche(m²);Rahmenanteil(%);Neigung(°);Azimut(°);Albedo(0-1);W_L;W_H;F_L1;F_L2;F_Br;F_St;F_Fo;Anzahl Fenster;Wandgruppe;Element;
        // X;Y;Z;0.0;0.0;0.0;

        // G1 Ost Wand;23;1;0;2;11;0;0;125;125;30;90;-115;0.1;17;14;0;0;0;0;0;0;3;10;

        public string Huellflaeche { get; set; } // beschreibung
        public int Huellflaechentyp { get; set; } // wert aus db
        public int Fenstertyp { get; set; } // wert aus db
        public int TWDTyp { get; set; } // wert aus db
        public int Diskretisierung { get; set; } // genauigkeit
        public int Wandart { get; set; } // aussen/innen/dach/boden/gekoppelt an zone
        public int Hypokauste { get; set; } // egal
        public int Abschattung { get; set; } // aussen
        public int Wandflaeche { get; set; } // brutto
        public int Fensterflaeche { get; set; } // brutto mit rahmen
        public int Rahmenanteil { get; set; } // 30%
        public int Neigung { get; set; } // klar 90 grad bei wand
        public int Azimut { get; set; } // klar in grad von sued
        public double Albedo { get; set; } // 30%
        public int W_L { get; set; } // wand laenge
        public int W_H { get; set; } // wand hoehe
        public int F_L1 { get; set; } // fenster
        public int F_L2 { get; set; } // fenster
        public int F_Br { get; set; } // fenster greite
        public int F_St { get; set; } // fenster sturz
        public int F_Fo { get; set; } // fenster
        public int AnzahlFenster { get; set; } // klar
        public int Wandgruppe { get; set; }
        public int Element { get; set; }
    }
}
