namespace csharp Sodao.Urljump.Service.Thrift

/**
* User对象
*/
struct User
{
	1:i64 UserId;// userid
	2:string UserName;// 注册账号
	3:string NickName;// 用户昵称
	4:i32 Gender;// 性别（1：男；2：女；0：未知）
	5:i64 RegTime;// 注册时间
}

/*service*/
service UrljumpService{
	/**
	* User_GetVoid使用方法
	*/
	 void User_GetVoid();

	 /**
	  * User_Get使用方法
	  * @param i64 userId - the string to print
	  * @return User - returns the i8/byte 'thing'
	  */
	 User User_Get(1:i64 userId);//根据uid获取用户信息

	 /**
	 * User_GetByName使用方法
	 */
	 User User_GetByName(1:i64 userId, 2:string userName);
}