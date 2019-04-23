using Bogus;
using ExpressWalker.Core.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ExpressWalker.Core.Test
{
    [TestClass]
    public class StressTest
    {
        [TestMethod]
        public void TypeWalker_StressTest_ComplexCircularReference_Build()
        {
            //Arrange

            var watch = new Stopwatch();
            
            //Act

            watch.Start();
            var visitor1 = TypeWalker<Document>.Create().ForProperty<DateTime>((x, m) => DateTime.Now).Build(10, new PropertyGuard(), false);
            watch.Stop();

            //Assert
            Assert.IsTrue(watch.ElapsedMilliseconds <= 1000);
        }

        [TestMethod]
        public void TypeWalker_StressTest_ComplexCircularReference_Visit()
        {
            //Arrange

            var watch = new Stopwatch();
            var visitor = TypeWalker<Document>.Create().ForProperty<DateTime>((x, m) => DateTime.Now.AddYears(10)).Build(10, new PropertyGuard(), false);
            var values = new HashSet<PropertyValue>();
            var document = GetComplexSample();

            //Act

            watch.Start();
            visitor.Visit(document, depth:10, guard:new InstanceGuard(), values:values);
            watch.Stop();

            //Assert
            Assert.IsTrue(values.Count == 100 && values.All(x => ((DateTime)x.NewValue).Year == DateTime.Now.Year + 10));
        }

        [TestMethod]
        public void TypeWalker_StressTest_AllowedHierarchy_Visit()
        {
            //Arrange

            var visitor = TypeWalker<AllowedHierarchy>.Create().ForProperty<DateTime>((x, m) => DateTime.Now.AddYears(10)).Build(10, new PropertyGuard());

            var hierarchy = new Faker<AllowedHierarchy>().StrictMode(false)
                                                 .CustomInstantiator(x => AllowedHierarchy.Build(6))
                                                 .Generate();

            var values = new HashSet<PropertyValue>();

            //Act

            visitor.Visit(hierarchy, depth: 10, guard: new InstanceGuard(), values: values);

            //Assert

            Assert.AreEqual(7, values.Count);
        }

        [TestMethod]
        public void TypeWalker_StressTest_SuppressedHierarchy_Visit()
        {
            //Arrange

            var visitor = TypeWalker<SuppressedHierarchy>.Create().ForProperty<DateTime>((x, m) => DateTime.Now.AddYears(10)).Build(10, new PropertyGuard());

            var hierarchy = new Faker<SuppressedHierarchy>().StrictMode(false)
                                                 .CustomInstantiator(x => SuppressedHierarchy.Build(6))
                                                 .Generate();

            var values = new HashSet<PropertyValue>();

            //Act

            visitor.Visit(hierarchy, depth: 10, guard: new InstanceGuard(), values: values);

            //Assert

            Assert.AreEqual(3, values.Count);
        }

        private Document GetComplexSample()
        {
            Func<UserToRole> newUserToRole = () => new Faker<UserToRole>().StrictMode(false)
                                                                          .RuleFor(x => x.UserToRole_User, new User())
                                                                          .RuleFor(x => x.UserToRole_Role, new Role())
                                                                          .Generate();

            

            Func<User> newUser = () => new Faker<User>().StrictMode(false)
                                                        .RuleFor(u => u.User_UserToRole, newUserToRole())
                                                        .Generate();

            Func<int, List<UserToRole>> newUserToRoleList = (int count) => new Faker<UserToRole>().StrictMode(false)
                                                                          .RuleFor(x => x.UserToRole_User, new User())
                                                                          .Generate(2);

            Func<int, List<OperationToProfile>> newOperationToProfileList = (int count) => new Faker<OperationToProfile>().StrictMode(false)
                                                                          .RuleFor(x => x.OperationToProfile_Operation, new Operation())
                                                                          .Generate(5);

            Func<Profile> newProfile = () => new Faker<Profile>().StrictMode(false)
                                                        .RuleFor(u => u.Profile_OperationsList, newOperationToProfileList(5))
                                                        .Generate();

            Func<int, List<Role>> newRoleList = (int count) => new Faker<Role>().StrictMode(false)
                                                        .RuleFor(r => r.Role_UserToRoleList, newUserToRoleList(2))
                                                        .RuleFor(r => r.Role_Profile, newProfile())
                                                        .Generate(count);

            Func<Unit> newUnit = () => new Faker<Unit>().StrictMode(false)
                                                        .RuleFor(u => u.Unit_Roles, newRoleList(2))
                                                        .Generate();

            var doc = new Faker<Document>().StrictMode(false)
                                            .RuleFor(x => x.DocumentDefaultUser1, newUser())
                                            .RuleFor(x => x.DocumentDefaultRole1, newRoleList(1).First())
                                            .RuleFor(x => x.DocumentDefaultUnit1, newUnit())
                                            .RuleFor(x => x.DocumentDefaultUnit2, Unit.Build(5))
                                            .Generate();

            return doc;
        }
    }

    public class Document
    {
        public User DocumentDefaultUser1 { get; set; }
        public Role DocumentDefaultRole1 { get; set; }
        public Unit DocumentDefaultUnit1 { get; set; }
        public User DocumentDefaultUser2 { get; set; }
        public Role DocumentDefaultRole2 { get; set; }
        public Unit DocumentDefaultUnit2 { get; set; }
        public DateTime TestDocumentDateTime1 { get; set; }
        public DateTime TestDocumentDateTime2 { get; set; }
        public DateTime TestDocumentDateTime3 { get; set; }
        public DateTime TestDocumentDateTime4 { get; set; }
        public DateTime TestDocumentDateTime5 { get; set; }

        public IList<CollectionItem> Items { get; set; }
    }

    public class CollectionItem
    {

    }

    

    public class User
    {
        public string TestUserString1 { get; set; }
        public string TestUserString2 { get; set; }
        public string TestUserString3 { get; set; }
        public string TestUserString4 { get; set; }
        public string TestUserString5 { get; set; }

        public DateTime TestUserDateTime1 { get; set; }
        public DateTime TestUserDateTime2 { get; set; }
        public DateTime TestUserDateTime3 { get; set; }
        public DateTime TestUserDateTime4 { get; set; }
        public DateTime TestUserDateTime5 { get; set; }

        public int TestUserInt1 { get; set; }
        public int TestUserInt2 { get; set; }
        public int TestUserInt3 { get; set; }
        public int TestUserInt4 { get; set; }
        public int TestUserInt5 { get; set; }

        public UserToRole User_UserToRole { get; set; }
    }

    public class Role
    {
        public string TestRoleString1 { get; set; }
        public string TestRoleString2 { get; set; }
        public string TestRoleString3 { get; set; }
        public string TestRoleString4 { get; set; }
        public string TestRoleString5 { get; set; }

        public DateTime TestRoleDateTime1 { get; set; }
        public DateTime TestRoleDateTime2 { get; set; }
        public DateTime TestRoleDateTime3 { get; set; }
        public DateTime TestRoleDateTime4 { get; set; }
        public DateTime TestRoleDateTime5 { get; set; }

        public int TestRoleInt1 { get; set; }
        public int TestRoleInt2 { get; set; }
        public int TestRoleInt3 { get; set; }
        public int TestRoleInt4 { get; set; }
        public int TestRoleInt5 { get; set; }

        public IList<UserToRole> Role_UserToRoleList { get; set; }

        public Profile Role_Profile { get; set; }
    }

    public class Unit
    {
        public string TestUnitString1 { get; set; }
        public string TestUnitString2 { get; set; }
        public string TestUnitString3 { get; set; }
        public string TestUnitString4 { get; set; }
        public string TestUnitString5 { get; set; }

        public DateTime TestUnitDateTime1 { get; set; }
        public DateTime TestUnitDateTime2 { get; set; }
        public DateTime TestUnitDateTime3 { get; set; }
        public DateTime TestUnitDateTime4 { get; set; }
        public DateTime TestUnitDateTime5 { get; set; }

        public int TestUnitInt1 { get; set; }
        public int TestUnitInt2 { get; set; }
        public int TestUnitInt3 { get; set; }
        public int TestUnitInt4 { get; set; }
        public int TestUnitInt5 { get; set; }

        [VisitorHierarchy]
        public Unit Unit_ParentUnit { get; set; }

        public IList<Role> Unit_Roles { get; set; }

        public static Unit Build(int depth)
        {
            var retVal = new Unit();
            var current = retVal;

            while (depth > 0)
            {
                current.Unit_ParentUnit = new Unit();
                current = current.Unit_ParentUnit;
                depth--;
            }

            return retVal;
        }
    }

    public class Profile
    {
        public string TestProfileString1 { get; set; }
        public string TestProfileString2 { get; set; }
        public string TestProfileString3 { get; set; }
        public string TestProfileString4 { get; set; }
        public string TestProfileString5 { get; set; }

        public DateTime TestProfileDateTime1 { get; set; }
        public DateTime TestProfileDateTime2 { get; set; }
        public DateTime TestProfileDateTime3 { get; set; }
        public DateTime TestProfileDateTime4 { get; set; }
        public DateTime TestProfileDateTime5 { get; set; }

        public int TestProfileInt1 { get; set; }
        public int TestProfileInt2 { get; set; }
        public int TestProfileInt3 { get; set; }
        public int TestProfileInt4 { get; set; }
        public int TestProfileInt5 { get; set; }

        public IList<OperationToProfile> Profile_OperationsList { get; set; }
    }

    public class UserToRole
    {
        public User UserToRole_User { get; set; }
        public Role UserToRole_Role { get; set; }
    }

    public class RoleToProfile
    {
        public Role UserToRole_Role { get; set; }
        public Profile UserToRole_Profile { get; set; }
    }

    public class Operation
    {
        public string TestOperationString1 { get; set; }
        public string TestOperationString2 { get; set; }
        public string TestOperationString3 { get; set; }
        public string TestOperationString4 { get; set; }
        public string TestOperationString5 { get; set; }

        public DateTime TestOperationDateTime1 { get; set; }
        public DateTime TestOperationDateTime2 { get; set; }
        public DateTime TestOperationDateTime3 { get; set; }
        public DateTime TestOperationDateTime4 { get; set; }
        public DateTime TestOperationDateTime5 { get; set; }

        public int TestOperationInt1 { get; set; }
        public int TestOperationInt2 { get; set; }
        public int TestOperationInt3 { get; set; }
        public int TestOperationInt4 { get; set; }
        public int TestOperationInt5 { get; set; }
    }

    public class OperationToProfile
    {
        public Operation OperationToProfile_Operation { get; set; }
        public Profile OperationToProfile_Profile { get; set; }
    }

    public class AllowedHierarchy
    {
        public int Depth { get; set; }

        public DateTime DateTime1 { get; set; }

        [VisitorHierarchy]
        public AllowedHierarchy Child { get; set; }

        public static AllowedHierarchy Build(int depth)
        {
            var retVal = new AllowedHierarchy();
            var current = retVal;

            while (depth > 0)
            {
                current.Child = new AllowedHierarchy();
                current = current.Child;
                depth--;
            }

            return retVal;
        }
    }

    public class SuppressedHierarchy
    {
        public DateTime DateTime1 { get; set; }

        //[VisitorHierarchy]
        public SuppressedHierarchy Child { get; set; }

        public static SuppressedHierarchy Build(int depth)
        {
            var retVal = new SuppressedHierarchy();
            var current = retVal;

            while (depth > 0)
            {
                current.Child = new SuppressedHierarchy();
                current = current.Child;
                depth--;
            }

            return retVal;
        }
    }
}
