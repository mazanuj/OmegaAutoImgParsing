using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json.Linq;
using xNet.Net;

namespace OmegaParsingLib
{
    public static class Initialize
    {
        public static async Task Parse(string filePath, string userName, string userPass)
        {
            try
            {
                //var req = await GetRequest(userName, userPass);
                //if (req == null)
                //    throw new Exception("Not logged in");

                Informer.RaiseOnResultReceived("Successfully logged in");

                var list = await ParseFile(filePath);
                if (list.Count < 2)
                    throw new Exception("Wrong file format");

                var firstRow = list[0];
                list.RemoveAt(0);

                await list.ForEachAsync(50, async val =>
                {
                    var req = await GetRequest(userName, userPass);
                    if (req == null)
                        throw new Exception("Not logged in");

                    var result = await DataParsing(req, val[0]);
                    if (string.IsNullOrEmpty(result))
                        val[11] = string.Empty;
                    else
                    {
                        val[11] = $"http://shop.omega-auto.biz/Images/img.ashx?gra_id={result}&tab_nr=255";
                        Informer.RaiseOnResultReceived($"{val[0]} => {val[11]}");
                    }
                });


                //for (var i = 1; i < list.Count; i++)
                //{
                //    var result = await DataParsing(req, list[i][0]);
                //    if (string.IsNullOrEmpty(result))
                //        list[i][11] = string.Empty;
                //    else
                //    {
                //        list[i][11] = $"http://shop.omega-auto.biz/Images/img.ashx?gra_id={result}&tab_nr=255";
                //        Informer.RaiseOnResultReceived($"{list[i][0]} => {list[i][11]}");
                //    }
                //}

                list.Insert(0, firstRow);
                await SaveFile(list, filePath);
            }
            catch (Exception ex)
            {
                Informer.RaiseOnResultReceived(ex);
            }
        }

        private static async Task<string> DataParsing(HttpRequest req, string search)
        {
            return await Task.Run(() =>
            {
                var result = string.Empty;
                try
                {
                    var url = $@"http://shop.omega-auto.biz/Articles/SearchDirectClick?filter.Card={search}";
                    var resp = req.Get(url).ToString();

                    var json = JObject.Parse(resp);
                    result = json["data"][0]["Id"].Value<string>();
                }
                catch (Exception ex)
                {
                    Informer.RaiseOnResultReceived(
                        $"Ваш поисковый запрос не вернул ни одного результата для карточки {search}");
                }
                return result;
            });
        }

        private static async Task<HttpRequest> GetRequest(string userName, string userPass)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var doc = new HtmlDocument();
                    var req = new HttpRequest
                    {
                        Cookies = new CookieDictionary(),
                        //Proxy = new HttpProxyClient("127.0.0.1", 8888)
                    };
                    const string url = "http://shop.omega-auto.biz/Account/Login.aspx?ReturnUrl=%2f";

                    var resp = req.Get(url).ToString();
                    doc.LoadHtml(resp);

                    var __VIEWSTATE = doc.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", string.Empty);
                    var __PREVIOUSPAGE = doc.GetElementbyId("__PREVIOUSPAGE").GetAttributeValue("value", string.Empty);
                    var __EVENTVALIDATION = doc.GetElementbyId("__EVENTVALIDATION")
                        .GetAttributeValue("value", string.Empty);

                    req.AddParam(@"__EVENTTARGET", "");
                    req.AddParam(@"__EVENTARGUMENT", "");
                    req.AddParam(@"TextField2", "");
                    req.AddParam(@"PasswordField", "");
                    req.AddParam(@"TextField1", "");
                    req.AddParam(@"LoginUser$LoginButton", "Войти");
                    req.AddParam(@"LoginUser$Password", userPass);
                    req.AddParam(@"LoginUser$UserName", userName);
                    req.AddParam(@"__EVENTVALIDATION", __EVENTVALIDATION);
                    req.AddParam(@"__PREVIOUSPAGE", __PREVIOUSPAGE);
                    req.AddParam(@"__VIEWSTATE", __VIEWSTATE);

                    req.Post(url).None();
                    //req.Get("http://shop.omega-auto.biz/Home/").None();

                    return req;
                }
                catch (Exception ex)
                {
                    Informer.RaiseOnResultReceived(ex);
                    return null;
                }
            });
        }

        private static async Task SaveFile(IEnumerable<List<string>> data, string filePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    var stringBuilder = new StringBuilder();
                    foreach (
                        var res in
                            data.Select(val => string.Join(",", val.Select(x => $"\"{x.ToString()}\"").ToArray())))
                        stringBuilder.AppendLine(res);

                    File.WriteAllText(filePath, stringBuilder.ToString());
                    Informer.RaiseOnResultReceived("File successfully saved");
                }
                catch (Exception ex)
                {
                    Informer.RaiseOnResultReceived($"File saved error: {ex.Message}");
                }
            });
        }

        private static async Task<List<List<string>>> ParseFile(string filePath)
        {
            return await Task.Run(() =>
            {
                var list = new List<List<string>>();
                try
                {
                    using (var parser = new TextFieldParser(filePath))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");
                        while (!parser.EndOfData)
                        {
                            //Processing row
                            var fields = parser.ReadFields();
                            if (fields == null)
                                throw new Exception("Wrong csv format");

                            list.Add(fields.ToList());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Informer.RaiseOnResultReceived(ex);
                }
                return list;
            });
        }
    }
}