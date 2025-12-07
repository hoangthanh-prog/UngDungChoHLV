using Microsoft.VisualStudio.TestTools.UnitTesting;
using QL_Gym_HLV.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QL_Gym_HLV.Tests
{
    [TestClass]
    public class QL_Gym_HLVControllerTests
    {
        [TestMethod]
        public void HLV_DangNhap_ReturnsView_WhenSessionIsNull()
        {
            var controller = new QL_Gym_HLVController();
            controller.Session["hlv"] = null;

            var result = controller.HLV_DangNhap() as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("HLV_DangNhap", result.ViewName);
        }
        [TestMethod]
        public void Error_ReturnsErrorView()
        {
            var controller = new QL_Gym_HLVController();

            var result = controller.Error() as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Error", result.ViewName);
        }
        [TestMethod]
        public void HLV_DangXuat_ClearsSessionAndReturnsLoginView()
        {
            var controller = new QL_Gym_HLVController();
            controller.Session["hlv"] = new object(); // giả lập session có dữ liệu

            var result = controller.HLV_DangXuat() as ViewResult;

            Assert.IsNull(controller.Session["hlv"]); // kiểm tra session đã bị xóa
            Assert.IsNotNull(result);
            Assert.AreEqual("HLV_DangNhap", result.ViewName);
        }

        [TestMethod]
        public void ChinhSuaLop_ReturnsPartialViewWithModel()
        {
            var controller = new QL_Gym_HLVController();

            var result = controller.ChinhSuaLop(1) as PartialViewResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(QL_Gym_HLV.Models.LopHoc));
        }

    }
}
