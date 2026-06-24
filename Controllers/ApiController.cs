using ApiMonitor.Data;
using ApiMonitor.Models;
using ApiMonitor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiMonitor.Controllers;

public class ApiController : Controller
{
    private readonly AppDbContext _db;
    private readonly ApiSyncService _syncService;
    private readonly ApiConnectionService _connectionService;

    public ApiController(AppDbContext db,
                         ApiSyncService syncService,
                         ApiConnectionService connectionService)
    {
        _db                = db;
        _syncService       = syncService;
        _connectionService = connectionService;
    }

    // -------------------------------------------------------
    // INDEX — list all APIs from DB
    // -------------------------------------------------------
    public async Task<IActionResult> Index()
    {
        var apis = await _db.Apis
            .Include(a => a.Endpoints)
            .OrderBy(a => a.Name)
            .ToListAsync();

        return View(apis);
    }

    // -------------------------------------------------------
    // DETAILS — one API with all endpoints
    // -------------------------------------------------------
    public async Task<IActionResult> Details(int id)
    {
        var api = await _db.Apis
            .Include(a => a.Endpoints)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (api == null) return NotFound();

        // Test connection every time the detail page is opened
        await _connectionService.TestAllEndpointsAsync(api);
        await _db.SaveChangesAsync();

        return View(api);
    }

    // -------------------------------------------------------
    // CREATE — manual entry (not from YAML)
    // -------------------------------------------------------
    [HttpGet]
    public IActionResult Create() => View(new Api());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Api api)
    {
        if (!ModelState.IsValid) return View(api);

        // Test connection before saving
        var result = await _connectionService.TestAsync(api.Url);
        api.StatusCode     = result.StatusCode;
        api.ResponseTimeMs = result.ResponseTimeMs;
        api.IsRunning      = result.IsRunning;

        _db.Apis.Add(api);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Added \"{api.Name}\" — {result.StatusCode} in {result.ResponseTimeMs}ms";
        return RedirectToAction(nameof(Index));
    }

    // -------------------------------------------------------
    // EDIT
    // -------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var api = await _db.Apis.Include(a => a.Endpoints)
                                .FirstOrDefaultAsync(a => a.Id == id);
        if (api == null) return NotFound();
        return View(api);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Api api)
    {
        if (id != api.Id) return BadRequest();
        if (!ModelState.IsValid) return View(api);

        // Test connection on every edit save
        var result = await _connectionService.TestAsync(api.Url);
        api.StatusCode     = result.StatusCode;
        api.ResponseTimeMs = result.ResponseTimeMs;
        api.IsRunning      = result.IsRunning;

        _db.Update(api);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Updated \"{api.Name}\" — {result.StatusCode} in {result.ResponseTimeMs}ms";
        return RedirectToAction(nameof(Index));
    }

    // -------------------------------------------------------
    // DELETE
    // -------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var api = await _db.Apis.FindAsync(id);
        if (api == null) return NotFound();
        return View(api);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var api = await _db.Apis.FindAsync(id);
        if (api != null)
        {
            _db.Apis.Remove(api);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Deleted \"{api.Name}\"";
        }
        return RedirectToAction(nameof(Index));
    }

    // -------------------------------------------------------
    // TEST — AJAX endpoint called from JS to retest on-demand
    // Returns JSON so the UI can update status badges live
    // -------------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Test(int id)
    {
        var api = await _db.Apis.Include(a => a.Endpoints)
                                .FirstOrDefaultAsync(a => a.Id == id);
        if (api == null) return NotFound();

        await _connectionService.TestAllEndpointsAsync(api);
        await _db.SaveChangesAsync();

        return Json(new
        {
            api.IsRunning,
            api.StatusCode,
            api.ResponseTimeMs,
            endpoints = api.Endpoints.Select(ep => new
            {
                ep.Id,
                ep.Method,
                ep.Path,
                ep.IsRunning,
                ep.StatusCode,
                ep.ResponseTimeMs
            })
        });
    }

    // -------------------------------------------------------
    // SYNC — re-reads all YAML files and upserts the DB
    // -------------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Sync()
    {
        await _syncService.SyncAllAsync();
        TempData["Success"] = "YAML sync complete — all APIs tested.";
        return RedirectToAction(nameof(Index));
    }
}
