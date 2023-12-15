using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserData 
{
	public string UserName;
	public string Email;
	public string Id;

	public UserData()
	{
		UserName = "Default User";
		Email = string.Empty;
		Id = "DefaultUserID";
	}

	public UserData(string email, string id)
	{
		Email = email;
		Id = id;
		UserName = email;
	}




}
