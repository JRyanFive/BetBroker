using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace BcWin.Core.XML
{

    /// <summary>
    /// XMLHelper XML
    /// </summary>
    public class XMLHelper
    {
        #region XML

        public static void SaveToXML(object classObject, string filePath)
        {

            string xmlString = null;
            XmlSerializer xmlSerializer = new XmlSerializer(classObject.GetType());
            using (MemoryStream memoryStream = new MemoryStream())
            {
                xmlSerializer.Serialize(memoryStream, classObject);
                memoryStream.Position = 0;
                xmlString = new StreamReader(memoryStream).ReadToEnd();

                // // Save C# class object into Xml file
                XElement xElement = XElement.Parse(xmlString);
                xElement.Save(@filePath);
            }



        }

        public static string ConvertObjectToXMLString(object classObject)
        {
            string xmlString = null;
            XmlSerializer xmlSerializer = new XmlSerializer(classObject.GetType());
            using (MemoryStream memoryStream = new MemoryStream())
            {
                xmlSerializer.Serialize(memoryStream, classObject);
                memoryStream.Position = 0;
                xmlString = new StreamReader(memoryStream).ReadToEnd();

                // // Save C# class object into Xml file
                // XElement xElement = XElement.Parse(xmlString);
                //xElement.Save(@"C:\Users\Administrator\Desktop\userDetail.xml");
            }
            return xmlString;
        }
        public static T ConvertXmlStringtoObject<T>(string xmlString)
        {
            T classObject;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (StringReader stringReader = new StringReader(xmlString))
            {
                classObject = (T)xmlSerializer.Deserialize(stringReader);
            }
            return classObject;
            ////Note: you can also read xmlString from Xml file by using below code
            // XElement xmlObject = XElement.Load(@"C:\Users\Administrator\Desktop\userDetail.xml");
            //string xmlString = xmlObject.ToString();
        }


        /// <summary>
        /// XPathXmlNode.
        /// </summary>
        /// <param name="xmlFileName">XML()</param>
        /// <param name="xpath">XPath(:"////")</param>
        /// <returns>XmlNode</returns>
        public static XmlNode GetXmlNodeByXpath(string xmlFileName, string xpath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlFileName); //XML
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
                return xmlNode;
            }
            catch (Exception ex)
            {
                return null;
                //throw ex; //
            }
        }
        /// <summary>
        /// XPathXmlNodeList.
        /// </summary>
        /// <param name="xmlFileName">XML()</param>
        /// <param name="xpath">XPath(:"////")</param>
        /// <returns>XmlNodeList</returns>
        public static XmlNodeList GetXmlNodeListByXpath(string xmlFileName, string xpath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlFileName); //XML
                XmlNodeList xmlNodeList = xmlDoc.SelectNodes(xpath);
                return xmlNodeList;
            }
            catch (Exception ex)
            {
                return null;
                //throw ex; //
            }
        }
        /// <summary>
        /// XPathxmlAttributeNameXmlAttribute.
        /// </summary>
        /// <param name="xmlFileName">XML()</param>
        /// <param name="xpath">XPath(:"////</param>
        /// <param name="xmlAttributeName">xmlAttributeName</param>
        /// <returns>xmlAttributeName</returns>
        public static XmlAttribute GetXmlAttribute(string xmlFileName, string xpath, string xmlAttributeName)
        {
            string content = string.Empty;
            XmlDocument xmlDoc = new XmlDocument();
            XmlAttribute xmlAttribute = null;
            try
            {
                xmlDoc.Load(xmlFileName); //XML
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
                if (xmlNode != null)
                {
                    if (xmlNode.Attributes.Count > 0)
                    {
                        xmlAttribute = xmlNode.Attributes[xmlAttributeName];
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex; //
            }
            return xmlAttribute;
        }
        #endregion
        #region XML?
        /// <summary>
        /// XML
        /// </summary>
        /// <param name="xmlFileName">XML()</param>
        /// <param name="rootNodeName">XML()</param>
        /// <param name="version">XML(:"1.0")</param>
        /// <param name="encoding">XML</param>
        /// <param name="standalone">"yes""no",null,SaveXML</param>
        /// <returns>true,false</returns>
        public static bool CreateXmlDocument(string xmlFileName, string rootNodeName, string version, string encoding, string standalone)
        {
            bool isSuccess = false;
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration(version, encoding, standalone);
                XmlNode root = xmlDoc.CreateElement(rootNodeName);
                xmlDoc.AppendChild(xmlDeclaration);
                xmlDoc.AppendChild(root);
                xmlDoc.Save(xmlFileName);
                isSuccess = true;
            }
            catch (Exception ex)
            {
                throw ex; //
            }
            return isSuccess;
        }
        /// <summary>
        /// XPath(
        /// </summary>
        /// <param name="xmlFileName">XML()</param>
        /// <param name="xpath">XPath(:"////</param>
        /// <param name="xmlNodeName">xmlNodeName</param>
        /// <param name="innerText"></param>
        /// <param name="xmlAttributeName">xmlAttributeName</param>
        /// <param name="value"></param>
        /// <returns>true,false</returns>
        public static bool CreateXmlNodeByXPath(string xmlFileName, string xpath, string xmlNodeName, string innerText, string xmlAttributeName, string value)
        {
            bool isSuccess = false;
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlFileName); //XML
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
                if (xmlNode != null)
                {
                    //
                    XmlElement subElement = xmlDoc.CreateElement(xmlNodeName);
                    subElement.InnerXml = innerText;
                    //
                    if (!string.IsNullOrEmpty(xmlAttributeName) && !string.IsNullOrEmpty(value))
                    {
                        XmlAttribute xmlAttribute = xmlDoc.CreateAttribute(xmlAttributeName);
                        xmlAttribute.Value = value;
                        subElement.Attributes.Append(xmlAttribute);
                    }
                    xmlNode.AppendChild(subElement);
                }
                xmlDoc.Save(xmlFileName); //XML
                isSuccess = true;
            }
            catch (Exception ex)
            {
                throw ex; //
            }
            return isSuccess;
        }
        /// <summary>
        /// XPath(,)
        /// </summary>
        /// <param name="xmlFileName">XML()</param>
        /// <param name="xpath">XPath(:"////</param>
        /// <param name="xmlNodeName">xmlNodeName</param>
        /// <param name="innerText"></param>
        /// <returns>true,false</returns>
        public static bool CreateOrUpdateXmlNodeByXPath(string xmlFileName, string xpath, string xmlNodeName, string innerText)
        {
            bool isSuccess = false;
            bool isExistsNode = false;//
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlFileName); //XML
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
                if (xmlNode != null)
                {
                    //xpath
                    foreach (XmlNode node in xmlNode.ChildNodes)
                    {
                        if (node.Name.ToLower() == xmlNodeName.ToLower())
                        {
                            //
                            node.InnerXml = innerText;
                            isExistsNode = true;
                            break;
                        }
                    }
                    if (!isExistsNode)
                    {
                        //
                        XmlElement subElement = xmlDoc.CreateElement(xmlNodeName);
                        subElement.InnerXml = innerText;
                        xmlNode.AppendChild(subElement);
                    }
                }
                xmlDoc.Save(xmlFileName); //XML
                isSuccess = true;
            }
            catch (Exception ex)
            {
                throw ex; //
            }
            return isSuccess;
        }
        /// <summary>
        /// XPath(,)
        /// </summary>
        /// <param name="xmlFileName">XML()</param>
        /// <param name="xpath">XPath(:"////</param>
        /// <param name="xmlAttributeName">xmlAttributeName</param>
        /// <param name="value"></param>
        /// <returns>true,false</returns>
        public static bool CreateOrUpdateXmlAttributeByXPath(string xmlFileName, string xpath, string xmlAttributeName, string value)
        {
            bool isSuccess = false;
            bool isExistsAttribute = false;//
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlFileName); //XML
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
                if (xmlNode != null)
                {
                    //xpath
                    foreach (XmlAttribute attribute in xmlNode.Attributes)
                    {
                        if (attribute.Name.ToLower() == xmlAttributeName.ToLower())
                        {
                            //
                            attribute.Value = value;
                            isExistsAttribute = true;
                            break;
                        }
                    }
                    if (!isExistsAttribute)
                    {
                        //
                        XmlAttribute xmlAttribute = xmlDoc.CreateAttribute(xmlAttributeName);
                        xmlAttribute.Value = value;
                        xmlNode.Attributes.Append(xmlAttribute);
                    }
                }
                xmlDoc.Save(xmlFileName); //XML
                isSuccess = true;
            }
            catch (Exception ex)
            {
                throw ex; //
            }
            return isSuccess;
        }
        #endregion

        #region XML
        /// <summary>
        /// XPath()
        /// </summary>
        /// <param name="xmlFileName">XML()</param>
        /// <param name="xpath">XPath(:"////</param>
        /// <returns>true,false</returns>
        public static bool DeleteXmlNodeByXPath(string xmlFileName, string xpath)
        {
            bool isSuccess = false;
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlFileName); //XML
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
                if (xmlNode != null)
                {
                    //
                    xmlNode.ParentNode.RemoveChild(xmlNode);
                }
                xmlDoc.Save(xmlFileName); //XML
                isSuccess = true;
            }
            catch (Exception ex)
            {
                throw ex; //
            }
            return isSuccess;
        }
        /// <summary>
        /// XPathxmlAttributeName
        /// </summary>
        /// <param name="xmlFileName">XML()</param>
        /// <param name="xpath">XPath(:"////</param>
        /// <param name="xmlAttributeName">xmlAttributeName</param>
        /// <returns>true,false</returns>
        public static bool DeleteXmlAttributeByXPath(string xmlFileName, string xpath, string xmlAttributeName)
        {
            bool isSuccess = false;
            bool isExistsAttribute = false;
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlFileName); //XML
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
                XmlAttribute xmlAttribute = null;
                if (xmlNode != null)
                {
                    //xpath
                    foreach (XmlAttribute attribute in xmlNode.Attributes)
                    {
                        if (attribute.Name.ToLower() == xmlAttributeName.ToLower())
                        {
                            //
                            xmlAttribute = attribute;
                            isExistsAttribute = true;
                            break;
                        }
                    }
                    if (isExistsAttribute)
                    {
                        //
                        xmlNode.Attributes.Remove(xmlAttribute);
                    }
                }
                xmlDoc.Save(xmlFileName); //XML
                isSuccess = true;
            }
            catch (Exception ex)
            {
                throw ex; //
            }
            return isSuccess;
        }
        /// <summary>
        /// XPath
        /// </summary>
        /// <param name="xmlFileName">XML()</param>
        /// <param name="xpath">XPath(:"////</param>
        /// <returns>true,false</returns>
        public static bool DeleteAllXmlAttributeByXPath(string xmlFileName, string xpath)
        {
            bool isSuccess = false;
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlFileName); //XML
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xpath);
                if (xmlNode != null)
                {
                    //xpath
                    xmlNode.Attributes.RemoveAll();
                }
                xmlDoc.Save(xmlFileName); //XML
                isSuccess = true;
            }
            catch (Exception ex)
            {
                throw ex; //
            }
            return isSuccess;
        }
        #endregion
    }
}
