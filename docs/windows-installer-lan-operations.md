# Windows Installer LAN Operations Guide

This guide explains LAN enablement, certificate trust, rollback, and troubleshooting for installer-managed deployments.

## Default Security Posture

- Fresh installs are local-only.
- API remains loopback-only (`127.0.0.1`) in all modes.
- No LAN firewall allow rule is created by default.
- LAN mode is opt-in from `Settings`.

## Enabling LAN Access

1. Log in as an admin user.
2. Open `Settings`.
3. In `Network Access`, configure host and HTTPS port.
4. Enable LAN access and click `Apply LAN settings`.

Expected host changes:
- IIS HTTPS binding created/updated for configured host/port.
- Local certificate generated (or reused) for the binding.
- Private-profile firewall allow rule created for configured HTTPS port.

## Regenerating Certificates

1. Open `Settings > Network Access`.
2. Click `Regenerate certificate`.
3. Confirm new certificate thumbprint is shown.

When regenerated:
- Existing IIS HTTPS binding is re-pointed to new leaf certificate.
- Mobile devices may need trust refresh if they no longer trust the root/chain in use.

## Mobile Trust Setup (Home LAN)

For each mobile device:
1. Export or retrieve the local root certificate used by FamilyFinances host.
2. Install the certificate profile on the device.
3. Mark the certificate as trusted in device settings.
4. Open `https://<host>:<port>` from the same private network.

## Security Caveats

- Do not expose this deployment to public internet/WAN.
- Keep LAN mode off when not needed.
- Use private home network profile only.
- API port `5084` must never be opened in firewall rules.

## Rollback Procedure (Installer Repair/Reinstall)

If installer-managed host operations fail repeatedly:
1. Disable LAN mode in `Settings` (if possible).
2. Stop/remove installer-managed runtime (`msiexec /x FamilyFinances-v<version>-win-x64.msi`).
3. Reinstall MSI package and validate IIS/site/service provisioning.
4. Keep data directories for continuity and backup validation.
5. In CI/release, keep MSI publication as single path to avoid drift.

## Troubleshooting Checklist

### IIS site not reachable
- Check site `FamilyFinances.Web` state in IIS Manager.
- Check app pool `FamilyFinances.Web.AppPool` is running.
- Verify local HTTP binding exists (`localhost:5019`) for baseline access.

### API unavailable
- Check Windows Service `FamilyFinances.Api`.
- Confirm API bind remains `127.0.0.1:5084`.
- Check logs in runtime log directory.

### LAN toggle fails
- Verify app process has required privileges for host operations.
- Check PowerShell script path configured in `FF_HOSTOPS_SCRIPTS_ROOT`.
- Review server logs for LAN operation audit/error entries.

### Certificate or HTTPS errors
- Verify certificate thumbprint matches IIS binding.
- Confirm certificate subject/SAN includes configured host.
- Re-run certificate regeneration and test again.

### Firewall still blocks LAN
- Confirm profile is `Private` on host network adapter.
- Confirm allow rule exists for configured HTTPS port.
- Confirm no conflicting deny rules override the allow rule.
