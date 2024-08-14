using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS.Services
{
    public class ChiTietHoaDonSver
    {
        Pro131BhdtContext _context;
        public ChiTietHoaDonSver()
        {
            _context = new Pro131BhdtContext();
        }
        public List<ChiTietHoaDon> GetALLHDCT()
        {
            return _context.ChiTietHoaDons.ToList();
        }

        public List<ChiTietHoaDon> GetByid(Guid idhdct)
        {
            return _context.ChiTietHoaDons.Where(p => p.MaHoaDon == idhdct).ToList();
        }

        public string AddToBill(Guid billID, Guid productID, long price, int amount)
        {
            try
            {
                var check = _context.ChiTietHoaDons.FirstOrDefault(p => p.MaHoaDon == billID && p.MaSanPham == productID);

                if (check == null) // SP mới chưa có trong hóa đơn
                {
                    var details = new ChiTietHoaDon
                    {
                        MaChiTiet = Guid.NewGuid(),
                        MaHoaDon = billID,
                        MaSanPham = productID,
                        GiaSanPham = price,
                        SoLuong = amount
                    };
                    _context.ChiTietHoaDons.Add(details);
                    _context.SaveChanges();

                    // Update số lượng của sản phẩm trong kho
                    var product = _context.SanPhams.Find(productID);
                    product.SoLuongCon = product.SoLuongCon - amount;
                    _context.SanPhams.Update(product);
                    _context.SaveChanges();

                    return "Thêm thành công vào hóa đơn";
                }
                else // SP đã có trong hóa đơn rồi => Update số lượng
                {
                    check.SoLuong += amount; // update số lượng trong bill Details
                    _context.ChiTietHoaDons.Update(check);
                    _context.SaveChanges();

                    // Update số lượng của sản phẩm trong kho
                    var product = _context.SanPhams.Find(productID);
                    product.SoLuongCon -= amount;
                    _context.SanPhams.Update(product);
                    _context.SaveChanges();

                    return "Thêm mới và cộng dồn thành công";
                }
            }
            catch (Exception ex) 
            {
                return "Tạo thất bại" + ex.Message;
            }
            // Check xem SP đã nằm trong hóa đơn hay chưa
            
        }

        public string UpdateBillDetail(Guid detailID, long price, int amount)
        {
            var detail = _context.ChiTietHoaDons.Find(detailID);
            if (detail != null)
            {
                var originalAmount = detail.SoLuong;
                detail.GiaSanPham = price;
                detail.SoLuong = amount;
                _context.ChiTietHoaDons.Update(detail);
                _context.SaveChanges();

                // Update số lượng của sản phẩm trong kho
                var product = _context.SanPhams.Find(detail.MaSanPham);
                product.SoLuongCon += originalAmount - amount;
                _context.SanPhams.Update(product);
                _context.SaveChanges();

                return "Cập nhật thành công";
            }
            return "Chi tiết hóa đơn không tồn tại";
        }
        public void UpdateTotalAmount(Guid billID)
        {
            var details = _context.ChiTietHoaDons.Where(cthd => cthd.MaHoaDon == billID).ToList();
            long totalAmount = details.Sum(cthd => cthd.GiaSanPham * cthd.SoLuong);

            var bill = _context.HoaDons.Find(billID);
            if (bill != null)
            {
                // Kiểm tra và áp dụng giảm giá từ voucher
                if (!string.IsNullOrEmpty(bill.MaVoucher))
                {
                    var voucher = _context.Vouchers.FirstOrDefault(v => v.MaVoucher == bill.MaVoucher);
                    if (voucher != null && voucher.GiaTriGiam > 0)
                    {
                        long soTienGiam = totalAmount * voucher.GiaTriGiam / 100;
                        totalAmount = totalAmount - soTienGiam;
                    }
                }

                bill.TongTien = totalAmount;
                _context.HoaDons.Update(bill);
                _context.SaveChanges();
            }
        }
        public string RefundProduct(Guid machitiet, Guid masanpham)
        {

            var billDetail = _context.ChiTietHoaDons.Find(machitiet);
            if (billDetail == null)
            {
                return "Chi tiết hóa đơn không tồn tại.";
            }


            var product = _context.SanPhams.Find(masanpham);
            if (product == null)
            {
                return "Sản phẩm không tồn tại.";
            }

            product.SoLuongCon += billDetail.SoLuong;

            _context.ChiTietHoaDons.Remove(billDetail);

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();

            return "Hoàn trả sản phẩm thành công.";
        }
        public string HoanlaItongtien(Guid machitiet, Guid mahoadon, Guid masanpham)
        {
            var gia = _context.SanPhams.Find(masanpham);
            if (gia == null)
            {
                return "Sản Phẩm  không tồn tại.";
            }
            var soluong = _context.ChiTietHoaDons.Find(machitiet);
            if (soluong == null)
            {
                return "Chi tiết hóa đơn không tồn tại.";
            }
            var tongtien = _context.HoaDons.Find(mahoadon);
            if (tongtien == null)
            {
                return "Hóa đon ko tồn tại";
            }
            tongtien.TongTien = 0;
            _context.HoaDons.Update(tongtien);
            _context.SaveChanges();
            return "Tỏng tiền đã đc cập nhật";
        }


        public string DeleteBillDetail(Guid detailID)
        {
            var detail = _context.ChiTietHoaDons.Find(detailID);
            if (detail != null)
            {
                var product = _context.SanPhams.Find(detail.MaSanPham);
                product.SoLuongCon += detail.SoLuong; // Hoàn trả số lượng vào kho

                _context.ChiTietHoaDons.Remove(detail);
                _context.SaveChanges();

                _context.SanPhams.Update(product);
                _context.SaveChanges();

                return "Xóa thành công";
            }
            return "Chi tiết hóa đơn không tồn tại";
        }
        public string ThanhToanHoaDon(Guid hoaDonId, long soTienKhachTra)
        { 
               var hoaDon = _context.HoaDons.Find(hoaDonId);
                if (hoaDon == null)
                {
                    return "Hóa đơn không tồn tại.";
                }

                var chiTietHoaDons = _context.ChiTietHoaDons.Where(ct => ct.MaHoaDon == hoaDonId).ToList();
                if (!chiTietHoaDons.Any())
                {
                    return "Không có chi tiết hóa đơn để thanh toán.";
                }

                hoaDon.TongTien = chiTietHoaDons.Sum(ct => ct.GiaSanPham * ct.SoLuong);
                if (soTienKhachTra < hoaDon.TongTien)
                {
                    return "Số tiền khách trả không đủ để thanh toán hóa đơn.";
                }

                hoaDon.TrangThai = 1;
                _context.HoaDons.Update(hoaDon);

                _context.ChiTietHoaDons.RemoveRange(chiTietHoaDons); // Xóa tất cả chi tiết hóa đơn liên quan
                _context.SaveChanges();

                return "Thanh toán thành công.";
            
        }
    } 
}
