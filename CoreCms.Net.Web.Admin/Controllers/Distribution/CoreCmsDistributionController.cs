/***********************************************************************
 *            Project: CoreCms
 *        ProjectName: 核心内容管理系统                                
 *                Web: https://www.corecms.net                      
 *             Author: 大灰灰                                          
 *              Email: jianweie@163.com                                
 *         CreateTime: 2021/1/31 21:45:10
 *        Description: 暂无
 ***********************************************************************/

using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CoreCms.Net.Configuration;
using CoreCms.Net.Filter;
using CoreCms.Net.IServices;
using CoreCms.Net.Loging;
using CoreCms.Net.Model.Entities;
using CoreCms.Net.Model.Entities.Expression;
using CoreCms.Net.Model.FromBody;
using CoreCms.Net.Model.ViewModels.UI;
using CoreCms.Net.Utility.Extensions;
using CoreCms.Net.Utility.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace CoreCms.Net.Web.Admin.Controllers
{
    /// <summary>
    ///     分销商表
    /// </summary>
    [Description("分销商表")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    [RequiredErrorForAdmin]
    [Authorize(Permissions.Name)]
    public class CoreCmsDistributionController : Controller
    {
        private readonly ICoreCmsDistributionServices _coreCmsDistributionServices;
        private readonly ICoreCmsUserGradeServices _userGradeServices;
        private readonly ICoreCmsDistributionGradeServices _distributionGradeServices;
        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        ///     构造函数
        /// </summary>
        public CoreCmsDistributionController(IWebHostEnvironment webHostEnvironment
            , ICoreCmsDistributionServices coreCmsDistributionServices, ICoreCmsUserGradeServices userGradeServices, ICoreCmsDistributionGradeServices distributionGradeServices)
        {
            _webHostEnvironment = webHostEnvironment;
            _coreCmsDistributionServices = coreCmsDistributionServices;
            _userGradeServices = userGradeServices;
            _distributionGradeServices = distributionGradeServices;
        }

        #region 获取列表============================================================

        // POST: Api/CoreCmsDistribution/GetPageList
        /// <summary>
        ///     获取列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Description("获取列表")]
        public async Task<JsonResult> GetPageList()
        {
            var jm = new AdminUiCallBack();
            var pageCurrent = Request.Form["page"].FirstOrDefault().ObjectToInt(1);
            var pageSize = Request.Form["limit"].FirstOrDefault().ObjectToInt(30);
            var where = PredicateBuilder.True<CoreCmsDistribution>();
            //获取排序字段
            var orderField = Request.Form["orderField"].FirstOrDefault();
            Expression<Func<CoreCmsDistribution, object>> orderEx = orderField switch
            {
                "id" => p => p.id,
                "userId" => p => p.userId,
                "name" => p => p.name,
                "gradeId" => p => p.gradeId,
                "mobile" => p => p.mobile,
                "weixin" => p => p.weixin,
                "qq" => p => p.qq,
                "storeName" => p => p.storeName,
                "storeLogo" => p => p.storeLogo,
                "storeBanner" => p => p.storeBanner,
                "storeDesc" => p => p.storeDesc,
                "verifyStatus" => p => p.verifyStatus,
                "createTime" => p => p.createTime,
                "updateTime" => p => p.updateTime,
                "verifyTime" => p => p.verifyTime,
                "isDelete" => p => p.isDelete,
                _ => p => p.id
            };

            //设置排序方式
            var orderDirection = Request.Form["orderDirection"].FirstOrDefault();
            var orderBy = orderDirection switch
            {
                "asc" => OrderByType.Asc,
                "desc" => OrderByType.Desc,
                _ => OrderByType.Desc
            };
            //查询筛选

            //序列 int
            var id = Request.Form["id"].FirstOrDefault().ObjectToInt(0);
            if (id > 0) where = where.And(p => p.id == id);
            //用户Id int
            var userId = Request.Form["userId"].FirstOrDefault().ObjectToInt(0);
            if (userId > 0) where = where.And(p => p.userId == userId);
            //分销商名称 nvarchar
            var name = Request.Form["name"].FirstOrDefault();
            if (!string.IsNullOrEmpty(name)) where = where.And(p => p.name.Contains(name));
            //分销等级 int
            var gradeId = Request.Form["gradeId"].FirstOrDefault().ObjectToInt(0);
            if (gradeId > 0) where = where.And(p => p.gradeId == gradeId);
            //手机号 nvarchar
            var mobile = Request.Form["mobile"].FirstOrDefault();
            if (!string.IsNullOrEmpty(mobile)) where = where.And(p => p.mobile.Contains(mobile));
            //微信号 nvarchar
            var weixin = Request.Form["weixin"].FirstOrDefault();
            if (!string.IsNullOrEmpty(weixin)) where = where.And(p => p.weixin.Contains(weixin));
            //qq号 nvarchar
            var qq = Request.Form["qq"].FirstOrDefault();
            if (!string.IsNullOrEmpty(qq)) where = where.And(p => p.qq.Contains(qq));
            //店铺名称 nvarchar
            var storeName = Request.Form["storeName"].FirstOrDefault();
            if (!string.IsNullOrEmpty(storeName)) where = where.And(p => p.storeName.Contains(storeName));
            //店铺Logo nvarchar
            var storeLogo = Request.Form["storeLogo"].FirstOrDefault();
            if (!string.IsNullOrEmpty(storeLogo)) where = where.And(p => p.storeLogo.Contains(storeLogo));
            //店铺Banner nvarchar
            var storeBanner = Request.Form["storeBanner"].FirstOrDefault();
            if (!string.IsNullOrEmpty(storeBanner)) where = where.And(p => p.storeBanner.Contains(storeBanner));
            //店铺简介 nvarchar
            var storeDesc = Request.Form["storeDesc"].FirstOrDefault();
            if (!string.IsNullOrEmpty(storeDesc)) where = where.And(p => p.storeDesc.Contains(storeDesc));
            //审核状态 int
            var verifyStatus = Request.Form["verifyStatus"].FirstOrDefault().ObjectToInt(0);
            if (verifyStatus > 0) where = where.And(p => p.verifyStatus == verifyStatus);
            //创建时间 datetime
            var createTime = Request.Form["createTime"].FirstOrDefault();
            if (!string.IsNullOrEmpty(createTime))
            {
                if (createTime.Contains("到"))
                {
                    var dts = createTime.Split("到");
                    var dtStart = dts[0].Trim().ObjectToDate();
                    where = where.And(p => p.createTime > dtStart);
                    var dtEnd = dts[1].Trim().ObjectToDate();
                    where = where.And(p => p.createTime < dtEnd);
                }
                else
                {
                    var dt = createTime.ObjectToDate();
                    where = where.And(p => p.createTime > dt);
                }
            }

            //更新时间 datetime
            var updateTime = Request.Form["updateTime"].FirstOrDefault();
            if (!string.IsNullOrEmpty(updateTime))
            {
                if (updateTime.Contains("到"))
                {
                    var dts = updateTime.Split("到");
                    var dtStart = dts[0].Trim().ObjectToDate();
                    where = where.And(p => p.updateTime > dtStart);
                    var dtEnd = dts[1].Trim().ObjectToDate();
                    where = where.And(p => p.updateTime < dtEnd);
                }
                else
                {
                    var dt = updateTime.ObjectToDate();
                    where = where.And(p => p.updateTime > dt);
                }
            }

            //审核时间 datetime
            var verifyTime = Request.Form["verifyTime"].FirstOrDefault();
            if (!string.IsNullOrEmpty(verifyTime))
            {
                if (verifyTime.Contains("到"))
                {
                    var dts = verifyTime.Split("到");
                    var dtStart = dts[0].Trim().ObjectToDate();
                    where = where.And(p => p.verifyTime > dtStart);
                    var dtEnd = dts[1].Trim().ObjectToDate();
                    where = where.And(p => p.verifyTime < dtEnd);
                }
                else
                {
                    var dt = verifyTime.ObjectToDate();
                    where = where.And(p => p.verifyTime > dt);
                }
            }

            //是否删除 bit
            var isDelete = Request.Form["isDelete"].FirstOrDefault();
            if (!string.IsNullOrEmpty(isDelete) && isDelete.ToLowerInvariant() == "true")
                where = where.And(p => p.isDelete);
            else if (!string.IsNullOrEmpty(isDelete) && isDelete.ToLowerInvariant() == "false")
                where = where.And(p => p.isDelete == false);
            //获取数据
            var list = await _coreCmsDistributionServices.QueryPageAsync(where, orderEx, orderBy, pageCurrent,
                pageSize);
            //返回数据
            jm.data = list;
            jm.code = 0;
            jm.count = list.TotalCount;
            jm.msg = "数据调用成功!";
            return Json(jm);
        }

        #endregion

        #region 首页数据============================================================

        // POST: Api/CoreCmsDistribution/GetIndex
        /// <summary>
        ///     首页数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Description("首页数据")]
        public async Task<JsonResult> GetIndex()
        {
            //返回数据
            var jm = new AdminUiCallBack { code = 0 };
            var distributionVerifyStatus = EnumHelper.EnumToList<GlobalEnumVars.DistributionVerifyStatus>();
            var grades = await _distributionGradeServices.GetCaChe();
            jm.data = new
            {
                distributionVerifyStatus,
                grades
            };

            return Json(jm);
        }

        #endregion

        #region 编辑数据============================================================

        // POST: Api/CoreCmsDistribution/GetEdit
        /// <summary>
        ///     编辑数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [Description("编辑数据")]
        public async Task<JsonResult> GetEdit([FromBody] FMIntId entity)
        {
            var jm = new AdminUiCallBack();

            var model = await _coreCmsDistributionServices.QueryByIdAsync(entity.id);
            if (model == null)
            {
                jm.msg = "不存在此信息";
                return Json(jm);
            }

            var distributionVerifyStatus = EnumHelper.EnumToList<GlobalEnumVars.DistributionVerifyStatus>();
            var grades = await _distributionGradeServices.GetCaChe();

            jm.code = 0;
            jm.data = new
            {
                model,
                distributionVerifyStatus,
                grades
            };

            return Json(jm);
        }

        #endregion

        #region 编辑提交============================================================

        // POST: Api/CoreCmsDistribution/Edit
        /// <summary>
        ///     编辑提交
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [Description("编辑提交")]
        public async Task<JsonResult> DoEdit([FromBody] CoreCmsDistribution entity)
        {
            var jm = new AdminUiCallBack();

            var oldModel = await _coreCmsDistributionServices.QueryByIdAsync(entity.id);
            if (oldModel == null)
            {
                jm.msg = "不存在此信息";
                return Json(jm);
            }

            //事物处理过程开始
            oldModel.name = entity.name;
            oldModel.gradeId = entity.gradeId;
            oldModel.mobile = entity.mobile;
            oldModel.weixin = entity.weixin;
            oldModel.qq = entity.qq;
            oldModel.verifyStatus = entity.verifyStatus;
            oldModel.updateTime = DateTime.Now;
            if (oldModel.verifyStatus == (int)GlobalEnumVars.DistributionVerifyStatus.VerifyYes) oldModel.verifyTime = DateTime.Now;

            //事物处理过程结束
            var bl = await _coreCmsDistributionServices.UpdateAsync(oldModel);
            jm.code = bl ? 0 : 1;
            jm.msg = bl ? GlobalConstVars.EditSuccess : GlobalConstVars.EditFailure;

            return Json(jm);
        }

        #endregion

        #region 删除数据============================================================

        // POST: Api/CoreCmsDistribution/DoDelete/10
        /// <summary>
        ///     单选删除
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [Description("单选删除")]
        public async Task<JsonResult> DoDelete([FromBody] FMIntId entity)
        {
            var jm = new AdminUiCallBack();

            var model = await _coreCmsDistributionServices.QueryByIdAsync(entity.id);
            if (model == null)
            {
                jm.msg = GlobalConstVars.DataisNo;
                return Json(jm);
            }

            var bl = await _coreCmsDistributionServices.DeleteByIdAsync(entity.id);
            jm.code = bl ? 0 : 1;
            jm.msg = bl ? GlobalConstVars.DeleteSuccess : GlobalConstVars.DeleteFailure;
            return Json(jm);

        }

        #endregion

        #region 预览数据============================================================

        // POST: Api/CoreCmsDistribution/GetDetails/10
        /// <summary>
        ///     预览数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [Description("预览数据")]
        public async Task<JsonResult> GetDetails([FromBody] FMIntId entity)
        {
            var jm = new AdminUiCallBack();

            var model = await _coreCmsDistributionServices.QueryByIdAsync(entity.id);
            if (model == null)
            {
                jm.msg = "不存在此信息";
                return Json(jm);
            }

            var distributionVerifyStatus = EnumHelper.EnumToList<GlobalEnumVars.DistributionVerifyStatus>();
            var grades = await _distributionGradeServices.GetCaChe();
            jm.code = 0;
            jm.data = new
            {
                model,
                distributionVerifyStatus,
                grades
            };

            return Json(jm);
        }

        #endregion
    }
}