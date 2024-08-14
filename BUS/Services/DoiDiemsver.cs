using DAL.Models;
using DAL.repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS.Services
{
    public class DoiDiemsver
    {
        DoiDiemLayVoucherRepo _repo;
        public DoiDiemsver()
        {
            _repo = new DoiDiemLayVoucherRepo();
        }
        public List<DoiDiemLayVoucher> GetAllDoiDiem()
        {
            return _repo.GetAllđoiiem();
        }

        public DoiDiemLayVoucher GetđoiiemById(Guid id)
        {
            return _repo.GetById(id);
        }
        public string DoiDiemLayVoucher(string soDienThoaiKhachHang, int soDiemCanDoi, string mavoucher)
        {
            return _repo.DoiDiemLayVoucher(soDienThoaiKhachHang, soDiemCanDoi, mavoucher);
        }
        public string CapNhatDiemTichLuy(string soDienThoaiKhachHang, int soDiemdoi)
        {
            return _repo.CapNhatDiemTichLuy(soDienThoaiKhachHang, soDiemdoi);
        }
        public string XoaDoiDiem(Guid id)
        {
            return _repo.DeleteĐoiiem(id);
        }
    }
}
