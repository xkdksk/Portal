using UJ.Data;
public partial class CharacterInfo{

	[System.NonSerialized]

	Str _Name;
	 public Str Name  {
		get{
			if(_Name==null){
				_Name= Str.FindStr(code ,1 );
			}
			 return _Name;
		}
	}


	[System.NonSerialized]

	Str _Desc;
	 public Str Desc  {
		get{
			if(_Desc==null){
				_Desc= Str.FindStr(code ,2 );
			}
			 return _Desc;
		}
	}

}
public partial class CharacterSkill{

	[System.NonSerialized]

	Str _Name;
	 public Str Name  {
		get{
			if(_Name==null){
				_Name= Str.FindStr(code ,3 );
			}
			 return _Name;
		}
	}


	[System.NonSerialized]

	Str _Desc;
	 public Str Desc  {
		get{
			if(_Desc==null){
				_Desc= Str.FindStr(code ,4 );
			}
			 return _Desc;
		}
	}

}
