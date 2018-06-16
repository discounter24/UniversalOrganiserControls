using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace UniversalOrganiserControls.Unturned3.RocketMod.Configuration
{
    public class RocketPermissionConfig
    {


        private DirectoryInfo rocketFolder;

        public List<Group> groups = new List<Group>();
        private XmlDocument RocketPermissionXML = new XmlDocument();


        public RocketPermissionConfig(DirectoryInfo rocketFolder)
        {
            this.rocketFolder = rocketFolder;

            if (rocketFolder.Exists)
            {
                FileInfo RocketPermissionFile = new FileInfo(rocketFolder.FullName + "\\Permissions.config.xml");
               
                if (RocketPermissionFile.Exists)
                {
                    RocketPermissionXML.Load(RocketPermissionFile.FullName);
                    load();
                }
                else
                {
                    throw new Exception("Configs not generated.");
                }

            }
            else
            {
                throw new Exception("Rocket folder does not exist.");
            }
        }


        private void load()
        {
            groups = new List<Group>();
            foreach(XmlNode group in RocketPermissionXML.SelectNodes("/RocketPermissions/Groups/Group"))
            {
                groups.Add(new Group(group));
            }


        }

        public void save()
        {
            try
            {
                
                FileInfo RocketPermissionFile = new FileInfo(rocketFolder + "\\Permissions.config.xml");
                
                RocketPermissionXML.Save(RocketPermissionFile.FullName);
            }
            catch (Exception)
            {
                throw;
            }

        }


        public string DefaultGroup
        {
            get
            {
                return RocketPermissionXML.SelectSingleNode("/RocketPermissions/DefaultGroup").InnerText;
            }
            set
            {
                RocketPermissionXML.SelectSingleNode("/RocketPermissions/DefaultGroup").InnerText = value;
            }
        }



        public Group createGroup(string ID)
        {

            XmlElement groupElement = RocketPermissionXML.CreateElement("Group");
            RocketPermissionXML.SelectSingleNode("/RocketPermissions/Groups").AppendChild(groupElement);


            Group g = new Group(groupElement);
            groups.Add(g);
            g.ID = ID;

            g.DisplayName = ID;
            g.ParentGroup = null;
            g.Color = "white";
            return g;
        }

        public void removeGroup(string group)
        {
            foreach (Group g in groups)
            {
                if (g.ID == group)
                {
                    g.getNode().ParentNode.RemoveChild(g.getNode());
                }
            }
            load();
        }


        public class Group
        {


            private XmlNode node;
            private List<Permission> permissions = new List<Permission>();
            private List<Member> members = new List<Member>();


            public Group(XmlNode group)
            {
                this.node = group;
                load();
            }

            public XmlNode getNode()
            {
                return node;
            }

            private void load()
            {
                permissions = new List<Permission>();
                try
                {
                    if (node.SelectSingleNode("Permissions") == null)
                    {
                        XmlElement permissionsNode = node.OwnerDocument.CreateElement("Permissions");
                        node.AppendChild(permissionsNode);
                    }
                    else
                    {
                        XmlNodeList permissionsNode = node.SelectNodes("Permissions/Permission");
                        foreach (XmlNode permission in permissionsNode)
                        {
                            permissions.Add(new Permission(permission));
                        }
                    }


                }
                catch (Exception)
                {
                    XmlElement permissionsNode = node.OwnerDocument.CreateElement("Permissions");
                    node.AppendChild(permissionsNode);
                }



                members = new List<Member>();
                try
                {
                    if (node.SelectSingleNode("Members") == null)
                    {
                        XmlElement permissionsNode = node.OwnerDocument.CreateElement("Members");
                        node.AppendChild(permissionsNode);
                    }
                    else
                    {
                        foreach (XmlNode member in node.SelectNodes("Members/Member"))
                        {
                            members.Add(new Member(member));
                        }
                    }
                }
                catch (Exception)
                {
                    XmlElement membersNode = node.OwnerDocument.CreateElement("Members");
                    node.AppendChild(membersNode);
                }

            }



            public string ID
            {
                get
                {
                    var n = node.SelectSingleNode("Id");
                    if (n != null)
                    {
                        return n.InnerText;
                    }
                    else
                    {
                        return "";
                    }
                }
                set
                {
                    string path = "Id";

                    var n = node.SelectSingleNode(path);
                    if (n == null & value != null)
                    {
                        n = node.OwnerDocument.CreateElement(path);
                        node.AppendChild(n);
                        n.InnerText = value;
                    }
                    else if (n != null & value == null)
                    {
                        n.ParentNode.RemoveChild(n);
                    }
                    else if (n != null & value != null)
                    {
                        n.InnerText = value;
                    }
                }
            }

            public string DisplayName
            {
                get
                {
                    var n = node.SelectSingleNode("DisplayName");
                    if (n != null)
                    {
                        return n.InnerText;
                    }
                    else
                    {
                        return "";
                    }
                }
                set
                {
                    string path = "DisplayName";

                    var n = node.SelectSingleNode(path);
                    if (n == null & value != null)
                    {
                        n = node.OwnerDocument.CreateElement(path);
                        node.AppendChild(n);
                        n.InnerText = value;
                    }
                    else if (n != null & value == null)
                    {
                        n.ParentNode.RemoveChild(n);
                    }
                    else if (n != null & value != null)
                    {
                        n.InnerText = value;
                    }
                }
            }

            public string Color
            {
                get
                {
                    var n = node.SelectSingleNode("Color");
                    if (n != null)
                    {
                        return n.InnerText;
                    }
                    else
                    {
                        return null;
                    }
                }
                set
                {
                    string path = "Color";

                    var n = node.SelectSingleNode(path);
                    if (n == null & value != null)
                    {
                        n = node.OwnerDocument.CreateElement(path);
                        node.AppendChild(n);
                        n.InnerText = value;
                    }
                    else if (n != null & value == null)
                    {
                        n.ParentNode.RemoveChild(n);
                    }
                    else if (n != null & value != null)
                    {
                        n.InnerText = value;
                    }
                }
            }

            public string ParentGroup
            {
                get
                {
                    var n = node.SelectSingleNode("ParentGroup");
                    if (n != null)
                    {
                        return n.InnerText;
                    }
                    else
                    {
                        return null;
                    }
                }
                set
                {
                    string path = "ParentGroup";

                    var n = node.SelectSingleNode(path);
                    if (n == null & value != null)
                    {
                        n = node.OwnerDocument.CreateElement(path);
                        node.AppendChild(n);
                        n.InnerText = value;
                    }
                    else if (n != null & value == null)
                    {
                        n.ParentNode.RemoveChild(n);
                    }
                    else if (n != null & value != null)
                    {
                        n.InnerText = value;
                    }
                }
            }


            public Permission addPermission(string permission, string cooldown)
            {
                XmlElement permissionNode = node.OwnerDocument.CreateElement("Permission");
                permissionNode.SetAttribute("Cooldown", cooldown);
                permissionNode.InnerText = permission;
   
                
                node.SelectSingleNode("Permissions").AppendChild(permissionNode);

                Permission p = new Permission(permissionNode);
                this.permissions.Add(p);
                return p;
                         
            }

            public void removePermission(string permission)
            {
                XmlNodeList permissionListNode = node.SelectNodes("Permissions/Permission");
                foreach (XmlNode permissionNode in permissionListNode)
                {
                    if (permissionNode.InnerText == permission)
                    {
                        permissionNode.ParentNode.RemoveChild(permissionNode);
                        permissions.RemoveAll(p => p.Command == permission);
                    }
                }
            }

            public List<Permission> getPermissions()
            {
                return permissions;
            }

            public Member addMember(string member)
            {
                XmlElement memberNode = node.OwnerDocument.CreateElement("Member");
                memberNode.InnerText = member;

                node.SelectSingleNode("Members").AppendChild(memberNode);



                Member m = new Member(memberNode);            
                members.Add(m);
                return m;
            }

            public void removeMember(string member)
            {
                XmlNodeList memberListNode = node.SelectNodes("Members/Member");
                foreach (XmlNode memberNode in memberListNode)
                {
                    if (memberNode.InnerText == member)
                    {
                        memberNode.ParentNode.RemoveChild(memberNode);
                        members.RemoveAll(m => m.MemberID == member);
                    }
                }
      
            }

            public List<Member> getMembers()
            {
                return members;
            }

        }


        public class Member
        {
            private XmlNode node;

            public Member(XmlNode node)
            {
                this.node = node;
            }

            public XmlNode getNode()
            {
                return node;
            }


            public string MemberID
            {
                get
                {
                    return node.InnerText;
                }
                set
                {
                    node.InnerText = value;
                }
            }
                

        }

        public class Permission
        {

            private XmlNode node;

      
            public string Cooldown
            {
                get
                {
                    return node.Attributes["Cooldown"]?.InnerText;
                }
                set
                {
                    node.Attributes["Cooldown"].InnerText = value;
                }
            }

            public string Command
            {
                get
                {
                    return node.InnerText;
                }
                set
                {
                    node.InnerText = value;
                }
            }

            public Permission(XmlNode node)
            {
                this.node = node;
            }

            public XmlNode getNode()
            {
                return node;
            }


        }

    }

}
