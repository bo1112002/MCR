//#define test

using fastJSON;
using MCR.Mods;
using MCR.Mods.VSTO;
using MCR.tool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Tools;

namespace MCR
{

    /// <summary>QST平台接口</summary>
    public class QST_Interface
    {
        /// <summary>获取接口调用的验证Token</summary>
        static string GetToken()
        {
            Dictionary<string, object> mapRS = null;
            string pathCache = AppSettings.Temps_DIR + "GetToken.cache";
            FileInfo file = new FileInfo(pathCache);
            if (file.Exists && DateTime.Now.AddMinutes(-100) <= file.LastWriteTime)
            {
                string strJson = File.ReadAllText(pathCache);
                mapRS = JSON.ToObject(strJson, typeof(Dictionary<string, object>)) as Dictionary<string, object>;
            }
            else
            {
                //boclassroom   密码 interface_bkt_1114
                string resultURL = AppSettings.QST("WebRoot").Val + "api/login";
                Dictionary<string, object> mapPostData = new Dictionary<string, object>();
                mapPostData.Add("password", AppSettings.QST("LoginPassword").Val);
                mapPostData.Add("username", AppSettings.QST("LoginName").Val);
                mapRS = PublicMethod.GetWebJsonString(resultURL, mapPostData);
                if (mapRS.ContainsKey("ERR") == false && mapRS["code"].ToString() == "200")
                {
                    File.Delete(pathCache);
                    string strJson = JSON.ToJSON(mapRS);
                    File.WriteAllText(pathCache, strJson);
                }
                else
                {
                    return string.Empty;
                }

            }

            Dictionary<string, object> mapData = mapRS["data"] as Dictionary<string, object>;
            string token = mapData["tokenId"].ToString();
            return token;
        }


        public static string GetToken2( string userID )
        {
            Dictionary<string, object> mapRS = null;
            string pathCache = AppSettings.Temps_DIR + "GetToken2.cache";
            FileInfo file = new FileInfo(pathCache);
            if (file.Exists && DateTime.Now.AddMinutes(-100) <= file.LastWriteTime)
            {
                string strJson = File.ReadAllText(pathCache);
                mapRS = JSON.ToObject(strJson, typeof(Dictionary<string, object>)) as Dictionary<string, object>;
            }
            else
            {
                string resultURL = AppSettings.QST("WebRoot").Val + "api/auth/tokens?methods=JWTToken";
                Dictionary<string, object> mapPostData = new Dictionary<string, object>();
                mapPostData.Add("userIdToGet", userID);
                mapRS = PublicMethod.GetWebJsonString(resultURL, mapPostData);
                if (mapRS.ContainsKey("ERR") == false && mapRS["code"].ToString() == "200")
                {
                    File.Delete(pathCache);
                    string strJson = JSON.ToJSON(mapRS);
                    File.WriteAllText(pathCache, strJson);
                }
                else
                {
                    return string.Empty;
                }

            }

            Dictionary<string, object> mapData = mapRS["data"] as Dictionary<string, object>;
            string token = mapData["tokenId"].ToString();
            return token;
        }


        public static Dictionary<string, object> CallbackURL_ToData(string url, object objPost)
        {
            string strJson = ConverterJson.CInstace().ToJson(objPost);
            byte[] bsBuff = Encoding.UTF8.GetBytes(strJson);

            Dictionary<string, object> rs = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, GetToken());
                req.Method = "POST";
                req.ContentType = "application/json";
                req.ContentLength = bsBuff.Length;
                req.GetRequestStream().Write(bsBuff, 0, bsBuff.Length);
            });

            return rs;
        }

        /// <summary>获取某个用户信息</summary>
        public static Dictionary<string, object> GetMemberInfo(string userID)
        {
            Dictionary<string, object> mapRS = null;
            string token = GetToken();
            if (string.IsNullOrEmpty(token))
            {
                mapRS = new Dictionary<string, object>();
                mapRS.Add("ERR", "无法获取验证标识(Token");
            }
            else
            {
                KeyValueClass kv = AppSettings.QST("WebRoot");
                //http://gatewaytev.eec-cn.com/course/api/boClassRoom/userInfos/ff8080816363863f016366a5bd4d0023
                string url = kv.Val + kv.V + "api/boClassRoom/userInfos/" + userID;
                mapRS = PublicMethod.GetWebJsonString(url, (req) =>
                {
                    req.Headers.Add(HttpRequestHeader.Authorization, token);
                });
            }
            return mapRS;
        }


        static readonly string _Key_GetList_School = "GetList_School_Key";
        /// <summary>获取学校集合信息</summary>
        public static Dictionary<string, object> GetList_School()
        {
            Dictionary<string, object> mapRS = EntityBase.GetMyICache().Get(_Key_GetList_School) as Dictionary<string, object>;
            if (mapRS != null)
                return mapRS;

            string token = GetToken();
            if (string.IsNullOrEmpty(token))
            {
                mapRS = new Dictionary<string, object>();
                mapRS.Add("ERR", "无法获取验证标识(Token");
            }
            else
            {
                KeyValueClass kv = AppSettings.QST("WebRoot");
                string url = kv.Val + kv.V + "api/boClassRoom/schools";
                mapRS = PublicMethod.GetWebJsonString(url, (req) =>
                {
                    req.Headers.Add(HttpRequestHeader.Authorization, token);
                });

                if (mapRS.ContainsKey("ERR") == false && mapRS["code"].ToString() == "200")
                {
                    EntityBase.GetMyICache().Set(_Key_GetList_School, mapRS, DateTime.Now.AddMinutes(60 * 10));
                }
            }
            return mapRS;
        }

        /// <summary>依据学校ID获某个学校信息接口</summary>
        public static Dictionary<string, object> GetSchoolByID(string schoolID)
        {
            Dictionary<string, object> mapRS = EntityBase.GetMyICache().Get(schoolID) as Dictionary<string, object>;
            if (mapRS != null)
                return mapRS;

            string token = GetToken();
            if (string.IsNullOrEmpty(token))
            {
                mapRS = new Dictionary<string, object>();
                mapRS.Add("ERR", "无法获取验证标识(Token");
            }
            else
            {
                KeyValueClass kv = AppSettings.QST("WebRoot");
                string url = kv.Val + kv.V + "api/boClassRoom/schools/" + schoolID;
                mapRS = PublicMethod.GetWebJsonString(url, (req) =>
                {
                    req.Headers.Add(HttpRequestHeader.Authorization, token);
                });

                if (mapRS.ContainsKey("ERR") == false && mapRS["code"].ToString() == "200")
                {
                    EntityBase.GetMyICache().Set(schoolID, mapRS, DateTime.Now.AddMinutes(60 * 10));
                }
            }
            return mapRS;
        }

        /// <summary>依据课程ID获取参与该课程的所有学生列表接口</summary>
        public static IList<WX_Member> GetList_StudentsByCourse(string courseID)
        {
            List<WX_Member> list = new List<WX_Member>();
            Dictionary<string, object> mapRS = null;
            string token = GetToken();
            if (string.IsNullOrEmpty(token) == false)
            {
                KeyValueClass kv = AppSettings.QST("WebRoot");
                string url = kv.Val + kv.V + "api/boClassRoom/courses/" + courseID + "/students";
                mapRS = PublicMethod.GetWebJsonString(url, (req) =>
                {
                    req.Headers.Add(HttpRequestHeader.Authorization, token);
                });

                if (mapRS.ContainsKey("data") && mapRS["code"].ToString() == "200")
                {
                    IDictionary dic = mapRS["data"] as IDictionary;
                    if (dic != null && dic.Contains("fieldList"))
                    {
                        IList listRs = dic["fieldList"] as IList;
                        if (listRs != null)
                        {
                            foreach (Dictionary<string, object> info in listRs)
                            {
                                string id = info["userInfoId"].ToString();
                                WX_Member theMember = WX_Member.GetByInnerID(id);
                                if (theMember != null)
                                {
                                    theMember.Tag = info["studentNo"].ToString();
                                    list.Add(theMember);

                                }
                            }
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>依据某老师的课程信息集合</summary>
        public static IList<CourseInfo_QST> GetList_CourseByTeacher(string teacherID)
        {
            string key = "GetList_CourseByTeacher_" + teacherID;
            List<CourseInfo_QST> listRs = EntityBase.GetMyICache().Get(key) as List<CourseInfo_QST>;
            if (listRs != null)
            {
                return listRs;
            }
            else
            {
                listRs = new List<CourseInfo_QST>();
            }

            Dictionary<string, object> mapRS = null;
#if test

            string pathDeom = @"G:\Micro_Classroom\MCR_New\MCR\aaa.txt";
            string strJson = File.ReadAllText(pathDeom);
            mapRS = ConverterJson.CInstace().ToObject(strJson, null) as Dictionary<string, object>;



#else

            KeyValueClass kv = AppSettings.QST("WebRoot");
            string url = kv.Val + kv.V + "api/boClassRoom/userInfos/" + teacherID + "/courses";
            mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, GetToken());
            });
#endif

            if (mapRS.ContainsKey("ERR") || mapRS["code"].ToString() != "200")
                return listRs;

            IList infos = mapRS["data"] as IList;
            foreach (object obj in infos)
            {
                Dictionary<string, object> info = obj as Dictionary<string, object>;
                if (info != null)
                {
                    CourseInfo_QST theCourse = new CourseInfo_QST(info);
                    EntityBase.GetMyICache().Set(theCourse.AutoID, theCourse, DateTime.Now.AddMinutes(60)); //缓存对象
                    listRs.Add(theCourse);
                }
            }

            EntityBase.GetMyICache().Set(key, listRs, DateTime.Now.AddMinutes(30)); //缓存集合
            return listRs;
        }



        /// <summary>依据某学生的课程信息集合</summary>
        public static IList<CourseInfo_QST> GetList_CourseByStudent(string studentID)
        {
            string key = "GetList_CourseByStudent" + studentID;
            IList<CourseInfo_QST> listRs = EntityBase.GetMyICache().Get(key) as IList<CourseInfo_QST>;
            if (listRs != null)
                return listRs;
            else
                listRs = new List<CourseInfo_QST>();


            Dictionary<string, object> mapRS = null;
#if test

            string pathDeom = @"G:\Micro_Classroom\MCR_New\MCR\aaa_A.txt";
            string strJson = File.ReadAllText(pathDeom);
            mapRS = ConverterJson.CInstace().ToObject(strJson, null) as Dictionary<string, object>;
#else
            KeyValueClass kv = AppSettings.QST("WebRoot");
            string url = kv.Val + kv.V + "api/boClassRoom/userInfos/" + studentID + "/courses/student";
            mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, GetToken());
            });
#endif



            if (mapRS.ContainsKey("ERR") || mapRS["code"].ToString() != "200")
                return listRs;

            IList infos = mapRS["data"] as IList;
            foreach (object obj in infos)
            {
                Dictionary<string, object> info = obj as Dictionary<string, object>;
                if (info != null)
                {
                    CourseInfo_QST theCourse = new CourseInfo_QST(info);
                    listRs.Add(theCourse);
                }
            }

            EntityBase.GetMyICache().Set(key, listRs, DateTime.Now.AddMinutes(30)); //缓存集合
            return listRs;
        }

        /// <summary>获取某课程信息</summary>
        public static CourseInfo_QST GetCourseByID(string courseID)
        {
            CourseInfo_QST theCourse = EntityBase.GetMyICache().Get(courseID) as CourseInfo_QST;
            if (theCourse != null)
                return theCourse;

            if (theCourse == null)
            {
                string strToken = GetToken();

                KeyValueClass kv = AppSettings.QST("WebRoot");
                string url = kv.Val + kv.V + "api/boClassRoom/courses/" + courseID;
                Dictionary<string, object> mapRS = PublicMethod.GetWebJsonString(url, (req) =>
               {
                   req.Headers.Add(HttpRequestHeader.Authorization, strToken);
               });
                if (mapRS.ContainsKey("ERR") || mapRS["code"].ToString() != "200")
                    return null;

                mapRS = mapRS["data"] as Dictionary<string, object>;
                theCourse = new CourseInfo_QST(mapRS);
                EntityBase.GetMyICache().Set(theCourse.AutoID, theCourse, DateTime.Now.AddMinutes(60));   //缓存对象
            }
            return theCourse;
        }


        /// <summary>依据某课程的章节信息集合</summary>
        public static IList<CourseDetail> GetList_CourseOfDetails(string courseID)
        {
            string key = "GetList_CourseOfDetails_" + courseID;
            IList<CourseDetail> list = EntityBase.GetMyICache().Get(key) as IList<CourseDetail>;
            if (list != null)
                return list;
            else
                list = new List<CourseDetail>();


            Dictionary<string, object> mapRS = null;
#if test

            string pathDeom = @"G:\Micro_Classroom\MCR_New\MCR\aaa2.txt";
            string strJson = File.ReadAllText(pathDeom);
            mapRS = ConverterJson.CInstace().ToObject(strJson, null) as Dictionary<string, object>;

#else

            KeyValueClass kv = AppSettings.QST("WebRoot");
            string url = kv.Val + kv.V + "api/boClassRoom/courses/" + courseID + "/chapters";
            mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, GetToken());
            });
#endif

            IList infos = null;
            if (mapRS.ContainsKey("ERR") || mapRS["code"].ToString() != "200")
                return list;


            if (mapRS["data"] is IList)
            {
                infos = mapRS["data"] as IList;
            }
            else
            {
                Dictionary<string, object> dirTemp = mapRS["data"] as Dictionary<string, object>;
                infos = dirTemp["fieldList"] as IList;
            }

            if(infos == null )
                return list;

            
            //if (dirTemp == null || dirTemp.ContainsKey("fieldList") == false)  return list;

            foreach (object obj in infos)
            {
                Dictionary<string, object> info = obj as Dictionary<string, object>;
                if (info != null)
                {
                    CourseDetail theDetaile = new CourseDetail(info);
                    list.Add(theDetaile);
                }
            }

            EntityBase.GetMyICache().Set(key, list, DateTime.Now.AddMinutes(60));
            return list;

        }


        /// <summary>依据ID的章节信息</summary>
        public static CourseDetail GetDetaileByID(string detaileID)
        {
            CourseDetail theDetaile = null;
            Dictionary<string, object> mapRS = null;
#if test

            string pathDeom = @"G:\Micro_Classroom\MCR_New\MCR\aaa3.txt";
            string strJson = File.ReadAllText(pathDeom);
            mapRS = ConverterJson.CInstace().ToObject(strJson, null) as Dictionary<string, object>;

#else
            string token = GetToken();

            KeyValueClass kv = AppSettings.QST("WebRoot");
            string url = kv.Val + kv.V + "api/boClassRoom/chapters/" + detaileID;
            mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, token);
            });
#endif

            if (mapRS.ContainsKey("data") && mapRS["code"].ToString() == "200")
            {
                Dictionary<string, object> dirInfo = mapRS["data"] as Dictionary<string, object>;
                theDetaile = new CourseDetail(dirInfo);
            }
            return theDetaile;
        }


        /// <summary>依据某章节ID，获取父级信息</summary>
        public static CourseDetail GetDetaileParentByID(string detaileID)
        {
            CourseDetail theDetaile = null;
            Dictionary<string, object> mapRS = null;
#if test

            string pathDeom = @"G:\Micro_Classroom\MCR_New\MCR\aaa4.txt";
            string strJson = File.ReadAllText(pathDeom);
            mapRS = ConverterJson.CInstace().ToObject(strJson, null) as Dictionary<string, object>;

#else
            string token = GetToken();
            KeyValueClass kv = AppSettings.QST("WebRoot");
            string url = kv.Val + kv.V + "api/boClassRoom/chapters/" + detaileID + "?Is_get_parent=true";
            mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, token);
            });
#endif

            if (mapRS.ContainsKey("data") && mapRS["code"].ToString() == "200")
            {

                if (mapRS["data"] is IList)
                {

                }
                else
                {
                    Dictionary<string, object> dirInfo = mapRS["data"] as Dictionary<string, object>;

                    if (dirInfo.ContainsKey("parentChapter"))
                    {
                        theDetaile = new CourseDetail(dirInfo["parentChapter"] as Dictionary<string, object>);
                    }
                }

            }
            return theDetaile;
        }
        /// <summary>依据某章节ID，获取父级信息(原始数据)</summary>
        public static Dictionary<string, object> GetDetaileParentByID2(string detaileID)
        {
            CourseDetail theDetaile = null;
            Dictionary<string, object> mapRS = null;
#if test1

            string pathDeom = @"G:\Micro_Classroom\MCR_New\MCR\aaa4.txt";
            string strJson = File.ReadAllText(pathDeom);
            mapRS = ConverterJson.CInstace().ToObject(strJson, null) as Dictionary<string, object>;

#else
            string token = GetToken();
            KeyValueClass kv = AppSettings.QST("WebRoot");
            string url = kv.Val + kv.V + "api/boClassRoom/chapters/" + detaileID + "?Is_get_parent=true";
            mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, token);
            });
#endif

            if (mapRS.ContainsKey("data") && mapRS["code"].ToString() == "200")
            {
                Dictionary<string, object> dirInfo = mapRS["data"] as Dictionary<string, object>;
                return dirInfo;
            }
            return null;
        }

        //http://gatewaydev.eec-cn.cn:8081/attachment/upload/attachment/classcase_ppt/FF833D0F0B97433081FE9781A7BBF3E8.pptx
        /// <summary>依据课程章节ID获取相应的ppt文件</summary>
        public static PPT_File_QST GetPPT(string courseDetailID)
        {
            courseDetailID = "ff8080815f528750015f7053069105ef";
            string token = GetToken();
            if (string.IsNullOrEmpty(token) == false)
            {
                KeyValueClass kv = AppSettings.QST("WebRoot");
                string url = kv.Val + kv.V + "api/boClassRoom/chapters/" + courseDetailID + "/pptResources";
                Dictionary<string, object> mapRS = PublicMethod.GetWebJsonString(url, (req) =>
                {
                    req.Headers.Add(HttpRequestHeader.Authorization, token);
                });
                if (mapRS.ContainsKey("data") && mapRS["code"].ToString() == "200")
                {
                    IDictionary dic = mapRS["data"] as IDictionary;
                    if (dic != null)
                    {
                        PPT_File_QST thePPT = new PPT_File_QST(dic);
                        return thePPT;
                    }
                }
            }
            return null;
        }

        /// <summary>获取筛选的习题库信息</summary>
        public static IList<QuestionInfo_QST> GetListQuestionByUserID(string userID, string qsnTypeIn, string keyword = "", int pageNum = 1)
        {
            List<QuestionInfo_QST> listQuest = new List<QuestionInfo_QST>();
#if test

            string pathDeom = @"G:\Micro_Classroom\MCR_New\MCR\aaa_GetListQuestionByUserID.json";
            string strJson = File.ReadAllText(pathDeom);
            Dictionary<string, object> mapRS =  ConverterJson.CInstace().ToObject(strJson, null) as Dictionary<string, object>;

#else

            keyword = System.Web.HttpUtility.UrlEncode(keyword);
            string token = GetToken();

            KeyValueClass kv = AppSettings.QST("WebRoot");
            string url = kv.Val + kv.V + "api/boClassRoom/libQuestions/teachers/" + userID
            + "?keyword=" + keyword + "&knowledge=&libraryType=0&pageNum=" + pageNum + "&qsnType=" + qsnTypeIn + "&subjectCode=&pageSize=20";
            Dictionary<string, object> mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, token);
            }, 200);

#endif
            if (mapRS.ContainsKey("data") && mapRS["code"].ToString() == "200")
            {
                Dictionary<string, object> info = mapRS["data"] as Dictionary<string, object>;
                IList list = info["fieldList"] as IList;
                QuestionInfo_QST.LoadList_QuestionInfo_QST(list, listQuest);
            }
            return listQuest;
        }

        /// <summary>依据题ID获取某题的详细信息</summary>
        public static IDictionary GetQuestionByID(string id)
        {
            Dictionary<string, object> info = null;
            string token = GetToken();
            KeyValueClass kv = AppSettings.QST("WebRoot");
            string url = kv.Val + kv.V + "api/boClassRoom/libQuestions/" + id;
            Dictionary<string, object> mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, token);
            });

            if (mapRS.ContainsKey("data") && mapRS["code"].ToString() == "200")
            {
                info = mapRS["data"] as Dictionary<string, object>;
            }

            return info;
        }



        /// <summary>依据班级ID获该班级下的所有学生列表接口</summary>
        public static IList<WX_Member> GetMembersByClassRoomID(string classRoomID)
        {
            List<WX_Member> listRS = new List<WX_Member>();
            string token = GetToken();
            KeyValueClass kv = AppSettings.QST("WebRoot");
            string url = kv.Val + kv.V + "api/boClassRoom/classInfos/" + classRoomID + "/students";
            Dictionary<string, object> mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, token);
            }, 200);

            if (mapRS.ContainsKey("data") && mapRS["code"].ToString() == "200")
            {
                IList list = mapRS["data"] as IList;
                if (list != null)
                {
                    foreach (IDictionary dic in list)
                    {
                        string uID = dic["userInfoId"].ToString();
                        WX_Member theMember = WX_Member.GetByInnerID(uID);
                        if (theMember != null)
                        {
                            listRS.Add(theMember);
                        }
                    }
                }

            }
            return listRS;
        }

        /// <summary>依据机构code查询所有所有用户</summary>
        public static IList<WX_Member> GetMembersByOrgCodeID(string orgCode)
        {
            List<WX_Member> listRS = new List<WX_Member>();

            string token = GetToken();
            KeyValueClass kv = AppSettings.QST("WebRoot");
            string url = kv.Val + kv.V + "api/boClassRoom/orgCode/" + orgCode + "/users";
            Dictionary<string, object> mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, token);
            }, 200);

            if (mapRS.ContainsKey("data") && mapRS["code"].ToString() == "200")
            {
                Dictionary<string, object> infos = mapRS["data"] as Dictionary<string, object>;
                IList list = infos["fieldList"] as IList;
                if (list != null)
                {
                    foreach (IDictionary dic in list)
                    {
                        string uID = dic["id"].ToString();
                        WX_Member theMember = WX_Member.GetByInnerID(uID);
                        if (theMember != null)
                        {
                            listRS.Add(theMember);
                        }
                    }
                }
            }
            return listRS;
        }



        /// <summary>获取某课程的作业信息列表</summary>
        public static IList<WorkInfo> GetWorksByCourseID(string courseID)
        {
            List<WorkInfo> listRS = new List<WorkInfo>();
            string token = GetToken();
            KeyValueClass kv = AppSettings.QST("WebRoot");
            //string url = kv.Val + kv.V + "api/homework?courseId=" + courseID;
            string url = kv.Val + kv.V + "api/homework?courseId=ff8080815ebc93b4015ebcdf6f4100a1";
            Dictionary<string, object> mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, token);
            }, 200);

            if (mapRS.ContainsKey("data") && mapRS["code"].ToString() == "200")
            {
                Dictionary<string, object> info = mapRS["data"] as Dictionary<string, object>;
                IList list = info["fieldList"] as IList;
                if (list != null)
                {
                    foreach (IDictionary dic in list)
                    {
                        try
                        {
                            WorkInfo wInfo = new WorkInfo(dic);
                            listRS.Add(wInfo);
                        }
                        catch { }
                    }
                }
            }
            return listRS;
        }


        /// <summary>获取某课程学生的作业信息列表</summary>
        public static IList<WorkInfo> GetWorksByCourseID_ToStduent(string courseID)
        {
            List<WorkInfo> listRS = new List<WorkInfo>();
            string token = GetToken();
            KeyValueClass kv = AppSettings.QST("WebRoot");
            string url = kv.Val + kv.V + "api/homework?courseId=" + courseID + "&status=2";
            //string url = kv.Val + kv.V + "api/homework?courseId=ff8080815ebc93b4015ebcdf6f4100a1&status=2";
            Dictionary<string, object> mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, token);
            }, 200);

            if (mapRS.ContainsKey("data") && mapRS["code"].ToString() == "200")
            {
                Dictionary<string, object> info = mapRS["data"] as Dictionary<string, object>;
                IList list = info["fieldList"] as IList;
                if (list != null)
                {
                    foreach (IDictionary dic in list)
                    {
                        try
                        {
                            WorkInfo wInfo = new WorkInfo(dic);
                            listRS.Add(wInfo);
                        }
                        catch { }
                    }
                }
            }
            return listRS;
        }


        /// <summary>获取某作业的信息(不存在则返回null)</summary>
        public static WorkInfo GetWorkInfo(string workID)
        {
            WorkInfo theWork = null;
            List<WorkInfo> listRS = new List<WorkInfo>();
            string token = GetToken();
            KeyValueClass kv = AppSettings.QST("WebRoot");
            string url = kv.Val + kv.V + "api/homework/" + workID;
            Dictionary<string, object> mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, token);
            }, 200);

            if (mapRS.ContainsKey("data") && mapRS["code"].ToString() == "200")
            {
                Dictionary<string, object> info = mapRS["data"] as Dictionary<string, object>;
                theWork = new WorkInfo(info);
            }
            return theWork;
        }

        /// <summary>获取某作业的信息(不存在则返回null)</summary>
        public static WorkInfo GetWorkInfo_ToStduent(string workID)
        {
            WorkInfo theWork = null;
            List<WorkInfo> listRS = new List<WorkInfo>();
            string token = GetToken();
            KeyValueClass kv = AppSettings.QST("WebRoot");
            string url = kv.Val + kv.V + "api/homework/" + workID + "/homeworkAnswerDetail";
            Dictionary<string, object> mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, token);
            }, 200);

            if (mapRS.ContainsKey("data") && mapRS["code"].ToString() == "200")
            {
                Dictionary<string, object> info = mapRS["data"] as Dictionary<string, object>;
                theWork = new WorkInfo(info);
            }
            return theWork;
        }




/// <summary>获取老师的ppt资源</summary>
        public static WorkInfo GetWorkInfo_ToStduent(string userID, int pageNumber=1 )
        {
            WorkInfo theWork = null;
            List<WorkInfo> listRS = new List<WorkInfo>();
            string token = GetToken();
            KeyValueClass kv = AppSettings.QST("WebRoot");
            string url = kv.Val + kv.V +  "api/boClassRoom/userInfoId/" + userID + "/pptResources?pageNumber=" + pageNumber + "&pageSize=10"; 
            //string url = "http://49.4.2.157:8081/course/api/boClassRoom/userInfoId/ff8080815f528750015f7053069105ef/pptResources?pageNumber=1&pageSize=10"; 
            Dictionary<string, object> mapRS = PublicMethod.GetWebJsonString(url, (req) =>
            {
                req.Headers.Add(HttpRequestHeader.Authorization, token);
            }, 200);

            if (mapRS.ContainsKey("data") && mapRS["code"].ToString() == "200")
            {
                Dictionary<string, object> info = mapRS["data"] as Dictionary<string, object>;
                theWork = new WorkInfo(info);
            }
            return theWork;
        }







    }






    [Serializable]
    public class BaseInfoClass
    {
        readonly Dictionary<string, object> _OtherInfos = new Dictionary<string, object>();
        /// <summary>其它信息项集合</summary>
        public Dictionary<string, object> OtherInfos
        {
            get { return _OtherInfos; }
        }

    }

    public class TokenClass
    {
        /// <summary>创建时间</summary>
        public DateTime CTime { get; private set; }
        /// <summary>Token字符串</summary>
        public string Token { get; private set; }

        public TokenClass(string token)
        {
            this.CTime = DateTime.Now;
            if (string.IsNullOrEmpty(token))
            {
                this.CTime = DateTime.Now.AddDays(-1);
            }

            this.Token = token;
        }

        /// <summary>是否过期</summary>
        public bool IsOutTime
        {
            get
            {
                if (string.IsNullOrEmpty(this.Token))
                    return true;

                return DateTime.Now.AddMinutes(-20) > this.CTime;
            }
        }

    }


    [Serializable]
    public class CourseInfo_QST : BaseInfoClass
    {

        #region MyRegion
        string _AutoID = string.Empty;
        /// <summary>物理标识号</summary>
        public string AutoID
        {
            get { return _AutoID; }
            set { _AutoID = value; }
        }
        string _Name = string.Empty;
        /// <summary>课程名</summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        int _Type = 0;
        /// <summary>课程类型</summary>
        public int Type
        {
            get { return _Type; }
            set { _Type = value; }
        }
        bool _IsTemplate = false;
        /// <summary>是否为模板</summary>
        public bool IsTemplate
        {
            get { return _IsTemplate; }
            set { _IsTemplate = value; }
        }

        int _Grade = 0;
        /// <summary>年级</summary>
        public int Grade
        {
            get { return _Grade; }
            set { _Grade = value; }
        }
        int _ClassHour = 0;
        /// <summary>教学课时</summary>
        public int ClassHour
        {
            get { return _ClassHour; }
            set { _ClassHour = value; }
        }
        DateTime _CreateTime = DateTime.Now;
        /// <summary>创建时间</summary>
        public DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
        string _Creator = string.Empty;
        /// <summary>创建者名称</summary>
        public string Creator
        {
            get { return _Creator; }
            set { _Creator = value; }
        }
        string _CreatorId = string.Empty;
        /// <summary>创建者ID</summary>
        public string CreatorId
        {
            get { return _CreatorId; }
            set { _CreatorId = value; }
        }
        string _CourseCode = string.Empty;
        /// <summary>课程编号</summary>
        public string CourseCode
        {
            get { return _CourseCode; }
            set { _CourseCode = value; }
        }
        bool _Disable = false;
        /// <summary>是否禁用</summary>
        public bool Disable
        {
            get { return _Disable; }
            set { _Disable = value; }
        }
        string _SpecialtyId = string.Empty;
        /// <summary>专业ID</summary>
        public string SpecialtyId
        {
            get { return _SpecialtyId; }
            set { _SpecialtyId = value; }
        }
        string _SpecialtyName = string.Empty;
        /// <summary>专业名称</summary>
        public string SpecialtyName
        {
            get { return _SpecialtyName; }
            set { _SpecialtyName = value; }
        }
        int _StudentCount = 0;
        /// <summary>学生数</summary>
        public int StudentCount
        {
            get { return _StudentCount; }
            set { _StudentCount = value; }
        }
        int _TaskCount = 0;
        /// <summary>任务数</summary>
        public int TaskCount
        {
            get { return _TaskCount; }
            set { _TaskCount = value; }
        }
        string _SubjectCode = string.Empty;
        /// <summary>科目ID</summary>
        public string SubjectCode
        {
            get { return _SubjectCode; }
            set { _SubjectCode = value; }
        }
        #endregion


        public string CreateTimeString
        {
            get
            {
                return this.CreateTime.ToString("yyyy-MM-dd");
            }
        }

        public CourseInfo_QST()
        { }

        public CourseInfo_QST(Dictionary<string, object> info)
        {
            this.AutoID = info["id"].ToString();
            this.Name = info["name"].ToString();
            this.Type = Convert.ToInt32(info["type"]);
            this.IsTemplate = Convert.ToBoolean(info["isTemplate"]);
            this.Grade = Convert.ToInt32(info["grade"]);
            this.ClassHour = Convert.ToInt32(info["classHour"]);
            //this.CreateTime = Convert.ToDateTime(info["createTime"]);
            this.Creator = Convert.ToString(info["creator"]);
            this.CreatorId = Convert.ToString(info["creatorId"]);
            this.CourseCode = Convert.ToString(info["courseCode"]);
            this.Disable = Convert.ToBoolean(info["disable"]);
            this.SpecialtyId = Convert.ToString(info["specialtyId"]);
            this.SpecialtyName = Convert.ToString(info["specialtyName"]);
            this.StudentCount = Convert.ToInt32(info["studentCount"]);
            this.SubjectCode = Convert.ToString(info["subjectCode"]);
        }


        public void ToEntity(Dictionary<string, object> info)
        {
            this.AutoID = info["AutoID"].ToString();
            this.Name = info["Name"].ToString();
            this.Type = Convert.ToInt32(info["Type"]);
            this.IsTemplate = Convert.ToBoolean(info["IsTemplate"]);
            this.Grade = Convert.ToInt32(info["Grade"]);
            this.ClassHour = Convert.ToInt32(info["ClassHour"]);
            this.CreateTime = Convert.ToDateTime(info["CreateTime"]);
            this.Creator = Convert.ToString(info["Creator"]);
            this.CreatorId = Convert.ToString(info["CreatorId"]);
            this.CourseCode = Convert.ToString(info["CourseCode"]);
            this.Disable = Convert.ToBoolean(info["Disable"]);
            this.SpecialtyId = Convert.ToString(info["SpecialtyId"]);
            this.SpecialtyName = Convert.ToString(info["SpecialtyName"]);
            this.StudentCount = Convert.ToInt32(info["StudentCount"]);
            this.TaskCount = Convert.ToInt32(info["TaskCount"]);
            this.SubjectCode = Convert.ToString(info["SubjectCode"]);
        }


        /// <summary>获取当前课程的章节明细集合</summary>
        public IList<CourseDetail> GetList_Details()
        {
            IList<CourseDetail> list = QST_Interface.GetList_CourseOfDetails(this.AutoID);
            return list;
        }


        /// <summary>获取某成员(学生或老师)的课程信息集合</summary>
        public static IList<CourseInfo_QST> GetAllByMember(WX_Member theMember)
        {
            IList<CourseInfo_QST> listRs = new List<CourseInfo_QST>();
            if (theMember.MType == MemberType.E_Student)
            {
                listRs = QST_Interface.GetList_CourseByStudent(theMember.InnerID);
            }
            else
            {
                listRs = QST_Interface.GetList_CourseByTeacher(theMember.InnerID);
            }
            return listRs;
        }

        /// <summary>依据ID获取某个课程对象</summary>
        public static CourseInfo_QST GetByID(string id)
        {
            if (string.IsNullOrEmpty(id) == true)
                return null;

            CourseInfo_QST the = QST_Interface.GetCourseByID(id);
            return the;
        }


    }


    /// <summary>章节明细对象</summary>
    [Serializable]
    public class CourseDetail
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public string CourseID { get; set; }
        public string ParentID { get; set; }


        readonly List<CourseDetail> _Children = new List<CourseDetail>();
        /// <summary>子集合</summary>
        public IList<CourseDetail> Children
        {
            get
            {
                return _Children;
            }
        }

        public CourseDetail()
        { }

        public CourseDetail(Dictionary<string, object> info)
        {
            this.Code = info["code"].ToString();
            this.Name = info["name"].ToString();
            this.ID = info["id"].ToString();

            if (info.ContainsKey("courseId"))
            {
                this.CourseID = info["courseId"].ToString();
            }
            else if (info.ContainsKey("course"))
            {
                this.CourseID = (info["course"] as IDictionary)["id"].ToString();
            }

            if (info.ContainsKey("parentId"))
            {
                this.ParentID = (info["parentId"] ?? string.Empty).ToString();
            }

            if (info.ContainsKey("children"))
            {
                IList list = info["children"] as IList;
                if (list != null)
                {
                    foreach (object obj in list)
                    {
                        Dictionary<string, object> theChilde = obj as Dictionary<string, object>;
                        if (theChilde == null)
                            continue;
                        CourseDetail theChild = new CourseDetail(theChilde);
                        _Children.Add(theChild);
                    }
                }
            }
        }


        public void ToEntity(Dictionary<string, object> info)
        {
            this.Code = info["Code"].ToString();
            this.Name = info["Name"].ToString();
            this.ID = info["ID"].ToString();
            this.CourseID = info["CourseID"].ToString();

            if (info.ContainsKey("Children"))
            {
                IList list = info["Children"] as IList;
                foreach (object obj in list)
                {
                    Dictionary<string, object> map_Childe = obj as Dictionary<string, object>;
                    if (map_Childe == null)
                        continue;
                    CourseDetail theChild = new CourseDetail();
                    theChild.ToEntity(map_Childe);
                    Children.Add(theChild);
                }
            }
        }

        /// <summary>获取当前所属的课堂信息</summary>
        public CourseInfo_QST GetCourseInfo_QST()
        {
            CourseInfo_QST the = CourseInfo_QST.GetByID(this.CourseID);
            return the;
        }

        /// <summary>获取上一级的信息名</summary>
        public string GetParentName()
        {
            CourseDetail theDetaile = QST_Interface.GetDetaileParentByID(this.ID);
            if (theDetaile != null)
                return theDetaile.Name;
            return string.Empty;
        }



    }

    /// <summary>ppt文件信息类</summary>
    [Serializable]
    public class PPT_File_QST
    {
        public string OriginFileName { get; set; }
        public string ID { get; set; }
        public string Url { get; set; }

        public PPT_File_QST() { }
        public PPT_File_QST(string fileName, string id, string url)
        {
            this.OriginFileName = fileName;
            this.ID = id;
            this.Url = url;
        }
        public PPT_File_QST(IDictionary dic)
        {
            this.OriginFileName = dic["originFileName"].ToString();
            this.ID = dic["id"].ToString();
            this.Url = dic["url"].ToString();
        }
    }

    /// <summary>题目</summary>
    [Serializable]
    public class QuestionInfo_QST
    {
        public string ID { get; set; }
        public string Code { get; set; }
        public string TitleText { get; set; }
        /// <summary>题目类型 1 单选 2多选 3 判断 5 编程 6 填空 7 简答</summary>
        public int QsnType { get; set; }
        public DateTime CreateTime { get; set; }
        public string QsnName { get; set; }
        /// <summary>难度(低，中，高)</summary>
        public string DifficultyName { get; set; }
        /// <summary>答案</summary>
        public string Answer { get; set; }

        /// <summary>分值</summary>
        public float Val
        {
            get
            {
                if (DifficultyName == "低")
                    return 1.0F;
                else if (DifficultyName == "中")
                    return 2.0F;
                else if (DifficultyName == "高")
                    return 3.0F;
                else
                    return 0F;
            }
        }


        List<QuestionInfoItem_QST> _Items = new List<QuestionInfoItem_QST>();
        /// <summary>题项集合</summary>
        public List<QuestionInfoItem_QST> Items
        {
            get { return _Items; }
            set { _Items = value; }
        }

        protected QuestionInfo_QST() { }

        protected QuestionInfo_QST(IDictionary info)
        {
            this.ID = info["id"].ToString();
            this.Code = info["code"].ToString();
            this.TitleText = info["titleText"].ToString();
            this.QsnType = Convert.ToInt32(info["qsnType"]);
            this.CreateTime = Convert.ToDateTime(info["createTime"]);
            this.QsnName = info["qsnName"].ToString();
            this.DifficultyName = info["difficultyName"].ToString();

            if (info.Contains("answer"))
            {
                this.Answer = (info["answer"] ?? string.Empty).ToString();
            }
        }


        public MC_QuestionClass Convert_MC_QuestionClass(string courseDetaileID)
        {
            MC_QuestionClass theMC = new MC_QuestionClass();

            theMC.Caption = this.TitleText;
            theMC.CourseDetaileID = courseDetaileID;
            theMC.CTime = this.CreateTime;
            theMC.GID = this.ID;

            if (this.QsnType == 2)
                theMC.QType = PPT_SlideType.Question_More;
            else if (this.QsnType == 1 || this.QsnType == 3)
                theMC.QType = PPT_SlideType.Question_One;

            theMC.Value = this.Val;

            foreach (QuestionInfoItem_QST item in this.Items)
            {
                MC_QuestionItemClass qItem = new MC_QuestionItemClass(string.Empty, item.OptionContent, item.IsVal);
                qItem.TagString = item.ShowKey;
                theMC.Items.Add(qItem);
            }
            return theMC;
        }


        public override string ToString()
        {
            string myAnswer = "A";
            if (this.QsnType == 3)
            {
                if (this.Answer != "true")
                    myAnswer = "B";

                return string.Format("[{0}] {1} ; [{2}] [{3}] [{4}]", this.QsnName, this.TitleText, this.Val, this.DifficultyName, myAnswer);
            }
            else
            {
                return string.Format("[{0}] {1} ; [{2}] [{3}] [{4}]", this.QsnName, this.TitleText, this.Val, this.DifficultyName, this.Answer);
            }
        }





        public static void LoadList_QuestionInfo_QST(IList listQ, IList<QuestionInfo_QST> loadList)
        {
            int it = 0;
            foreach (object obj in listQ)
            {
                it++;
                IDictionary info = obj as IDictionary;
                QuestionInfo_QST theQuest = new QuestionInfo_QST(info);

                if (theQuest.QsnType <= 0 || theQuest.QsnType > 3)
                    continue;

                if (theQuest.QsnType == 3)
                {
                    QuestionInfoItem_QST qItem_true = new QuestionInfoItem_QST();
                    qItem_true.OptionContent = "YES";
                    qItem_true.ShowKey = "A";
                    qItem_true.IsVal = (theQuest.Answer.ToLower() == "true");
                    theQuest.Items.Add(qItem_true);

                    QuestionInfoItem_QST qItem_false = new QuestionInfoItem_QST();
                    qItem_false.OptionContent = "NO";
                    qItem_false.ShowKey = "B";
                    qItem_false.IsVal = (theQuest.Answer.ToLower() != "true");
                    theQuest.Items.Add(qItem_false);

                }
                else
                {
                    IList listItem = info["options"] as IList;
                    if (listItem == null)
                        continue;

                    foreach (object obj2 in listItem)
                    {
                        IDictionary info2 = obj2 as IDictionary;
                        if (theQuest.QsnType == 1 || theQuest.QsnType == 2)
                        {
                            QuestionInfoItem_QST qItem = new QuestionInfoItem_QST();
                            qItem.OptionContent = info2["optionContent"].ToString();
                            qItem.ShowKey = info2["showKey"].ToString();
                            qItem.IsVal = (theQuest.Answer.Trim().Contains(qItem.ShowKey));
                            theQuest.Items.Add(qItem);
                        }
                    }
                }

                loadList.Add(theQuest);
            }
        }


        public static void LoadList_QuestionInfo_QST_ToEntity(IList listQ, IList<QuestionInfo_QST> loadList)
        {
            foreach (object obj in listQ)
            {
                IDictionary info = obj as IDictionary;
                QuestionInfo_QST the = new QuestionInfo_QST();
                the.ID = info["ID"].ToString();
                the.Code = info["Code"].ToString();
                the.TitleText = info["TitleText"].ToString();
                the.QsnType = Convert.ToInt32(info["QsnType"]);
                the.CreateTime = Convert.ToDateTime(info["CreateTime"]);
                the.QsnName = info["QsnName"].ToString();
                the.DifficultyName = info["DifficultyName"].ToString();
                the.Answer = (info["Answer"] ?? string.Empty).ToString();



                IList listItems = info["Items"] as IList;
                foreach (object obj2 in listItems)
                {
                    IDictionary info2 = obj2 as IDictionary;
                    QuestionInfoItem_QST the2 = new QuestionInfoItem_QST();

                    the2.OptionContent = info2["OptionContent"].ToString();
                    the2.ShowKey = info2["ShowKey"].ToString();
                    the2.IsVal = Convert.ToBoolean(info2["IsVal"]);
                    the.Items.Add(the2);
                }


                loadList.Add(the);
            }



        }

    }

    /// <summary>题项</summary>
    [Serializable]
    public class QuestionInfoItem_QST
    {
        internal QuestionInfoItem_QST()
        {
        }
        public string OptionContent { get; set; }
        public string ShowKey { get; set; }
        public bool IsVal { get; set; }


        public override string ToString()
        {
            return string.Format("{0} : {1}", this.ShowKey, this.OptionContent);
        }
    }
    /// <summary>作业信息对象</summary>
    [Serializable]
    public class WorkInfo
    {
        /// <summary>作业ID</summary>
        public string ID { get; private set; }
        /// <summary>课程ID</summary>
        public string CourseID { get; private set; }
        /// <summary>作业名</summary>
        public string Name { get; private set; }
        /// <summary>总分</summary>
        public float TotalScore { get; private set; }
        /// <summary>描述</summary>
        public string Description { get; private set; }
        /// <summary>结束时间</summary>
        public string EndTime { get; private set; }
        /// <summary>发布状态（1是未发布，2是已发布）</summary>
        public int Status { get; private set; }
        /// <summary>创建名</summary>
        public string CreatorName { get; private set; }
        /// <summary>创建者ID</summary>
        public string CreatorID { get; private set; }
        /// <summary>创建时间</summary>
        public string CreateTime { get; private set; }

        int _SubStatus = -1;
        /// <summary>学生是否提交作业</summary>
        public int SubStatus
        {
            get { return _SubStatus; }
            private set { _SubStatus = value; }
        }

        public WorkInfo() { }
        public WorkInfo(IDictionary dicInfo, bool isStudentInfo = false)
        {
            this.ID = dicInfo["id"].ToString();
            this.CourseID = dicInfo["courseId"].ToString();
            this.Name = dicInfo["name"].ToString();
            this.TotalScore = Convert.ToSingle(dicInfo["totalScore"]);
            this.Status = Convert.ToInt32(dicInfo["status"]);
            this.EndTime = (dicInfo["endTime"] ?? string.Empty).ToString();
            this.CreatorID = dicInfo["creatorId"].ToString();
            this.CreatorName = dicInfo["creator"].ToString();
            this.CreateTime = dicInfo["createTime"].ToString();
            this.Description = (dicInfo["description"] ?? string.Empty).ToString();


            if (dicInfo.Contains("homeworkQuestions") == true)
            {

                IList list2 = dicInfo["homeworkQuestions"] as IList;
                if (list2 != null)
                {
                    _WorkQuestions.Clear();
                    foreach (IDictionary dic2 in list2)
                    {
                        if (dic2 == null)
                            continue;
                        try
                        {
                            if (isStudentInfo == false)
                            {
                                WorkDetaileInfo wdInfo = new WorkDetaileInfo(dic2);
                                _WorkQuestions.Add(wdInfo);
                            }
                            else
                            {
                                WorkDetaileInfo wdInfo = new WorkDetaileInfo_Stdudent(dic2);
                                _WorkQuestions.Add(wdInfo);
                            }
                        }
                        catch { }
                    }
                }
            }

            if (dicInfo.Contains("subStatus"))
            {
                this.SubStatus = Convert.ToInt32((dicInfo["subStatus"] ?? "1").ToString());
            }

        }


        public string StatusString
        {
            get
            {
                if (SubStatus > 0) //表示当前信息为学生作业(已发布的作业)
                {
                    return this.SubStatus == 2 ? "已提交" : "未提交";
                }
                else
                {
                    return this.Status == 2 ? "已发布" : "未发布";
                }
            }
        }



        List<WorkDetaileInfo> _WorkQuestions = new List<WorkDetaileInfo>();
        /// <summary>作业题目集合</summary>
        public List<WorkDetaileInfo> WorkQuestions
        {
            get { return _WorkQuestions; }
        }


    }


    /// <summary>作业详情对象</summary>
    [Serializable]
    public class WorkDetaileInfo
    {
        /// <summary>题ID</summary>
        public string ID { get; private set; }
        /// <summary>作业ID</summary>
        public string WorkID { get; private set; }
        /// <summary>作业题目内容</summary>
        public string TitleText { get; private set; }
        /// <summary>分值</summary>
        public float EveryScore { get; private set; }
        /// <summary>题号</summary>
        public string OrderNumber { get; private set; }
        /// <summary>难度名</summary>
        public string DifficultyName { get; private set; }
        /// <summary>题型名</summary>
        public string QsnName { get; private set; }
        /// <summary>评分规则</summary>
        public string GradeRule { get; private set; }
        /// <summary>解析</summary>
        public string Tip { get; private set; }

        public WorkDetaileInfo() { }
        public WorkDetaileInfo(IDictionary dicInfo)
        {
            this.ID = dicInfo["id"].ToString();
            this.WorkID = dicInfo["homeworkId"].ToString();
            this.EveryScore = Convert.ToSingle(dicInfo["everyScore"]);
            this.OrderNumber = dicInfo["orderNumber"].ToString();
            this.DifficultyName = dicInfo["difficultyName"].ToString();
            this.QsnName = dicInfo["qsnName"].ToString();
            this.TitleText = dicInfo["titleText"].ToString();
            this.GradeRule = dicInfo["gradeRule"].ToString();
            this.Tip = dicInfo["tip"].ToString();
        }

    }


    /// <summary>作业详情对象</summary>
    [Serializable]
    public class WorkDetaileInfo_Stdudent : WorkDetaileInfo
    {
        /// <summary>学生答案</summary>
        public string UserScore { get; private set; }
        /// <summary>学生答案是否正确</summary>
        public int IsCorrect { get; private set; }
        

        public WorkDetaileInfo_Stdudent() { }
        public WorkDetaileInfo_Stdudent(IDictionary dicInfo)
            : base(dicInfo)
        {
            this.UserScore = dicInfo["userScore"].ToString();
            this.IsCorrect = Convert.ToInt32(dicInfo["isCorrect"] ?? "0");
        }
    }



}
