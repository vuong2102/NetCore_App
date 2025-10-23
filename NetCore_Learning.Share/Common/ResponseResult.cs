using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore_Learning.Share.Common
{
    public record ResponseResult<T>(T Result, string Code = null, string ErrorMessage = "");
    public record SuccessResult<T>(T Result = default(T)) : ResponseResult<T>(Result, ResultCode.SuccessResult, null);
    public record NotFoundRecordResult<T>(string ErrorMessage = "Không tìm thấy dữ liệu") : ResponseResult<T>(default(T), ResultCode.NotfoundResult, ErrorMessage);
    public record ExistRecordResult<T>(string ErrorMessage = "Dữ liệu đã tồn tại") : ResponseResult<T>(default(T), ResultCode.ExistRecordResult, ErrorMessage);
    public record InvalidDataResponseResult<T>(string ErrorMessage = "Dữ liệu không hợp lệ", dynamic ErrorMessageDetail = null) : ResponseResult<T>(default(T), ResultCode.InvalidDataResult, ErrorMessage);
    public record InactiveDataResult<T>(string ErrorMessage = "Dữ liệu chưa được kích hoạt", dynamic ErrorMessageDetail = null) : ResponseResult<T>(default(T), ResultCode.InactiveDataResult, ErrorMessage);
    public record PermissionMissingDataResult<T>(string ErrorMessage = "Không có quyền truy cập dữ liệu") : ResponseResult<T>(default(T), ResultCode.PermissionMissingDataResult, ErrorMessage);
    public record WarningResult<T>(string ErrorMessage = "Cảnh báo dữ liệu cần kiểm tra") : ResponseResult<T>(default(T), ResultCode.WarningResult, ErrorMessage);
    public static class ResultCode
    {
        public readonly static string SuccessResult = "00";
        public readonly static string InvalidDataResult = "01";
        public readonly static string NotfoundResult = "02";
        public readonly static string ExistRecordResult = "03";
        public readonly static string InactiveDataResult = "04";
        public readonly static string PermissionMissingDataResult = "05";
        public readonly static string WarningResult = "06";
        public readonly static string ContinueProcessResult = "07";
        public readonly static string ExceptionResult = "99";
    }
}
