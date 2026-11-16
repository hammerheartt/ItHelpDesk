import { useEffect, useState } from 'react'
import { Link } from 'react-router'
import { api, ApiError } from '../api/client'
import { TICKET_STATUSES, type Stats } from '../api/types'
import { formatHours, statusLabel } from '../utils/format'

export default function DashboardPage() {
  const [stats, setStats] = useState<Stats | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let ignore = false
    api<Stats>('/stats')
      .then(s => {
        if (!ignore) {
          setStats(s)
          setError(null)
        }
      })
      .catch(err => {
        if (!ignore) setError(err instanceof ApiError ? err.message : 'Cannot reach the server.')
      })
    return () => { ignore = true }
  })

  return (
    <>
      <div className="page-header">
        <h1>Dashboard</h1>
      </div>

      {error && <p className="error">{error}</p>}

      {stats && (
        <>
          <div className="stat-cards">
            <StatCard label="Unassigned" value={stats.unassignedTickets} />
            <StatCard label="Open" value={stats.openTickets} />
            <StatCard label="Overdue" value={stats.overdueTickets} warn={stats.overdueTickets > 0} />
            <StatCard label="Reopened" value={stats.reopenedTickets} />
            <StatCard label="Avg. resolution" value={formatHours(stats.averageResolutionHours)} />
            <StatCard label="Total" value={stats.totalTickets} />
          </div>

          <div className="dashboard-grid">
            <section className="panel">
              <h2>Tickets by status</h2>
              <BarList
                items={TICKET_STATUSES.map(s => ({
                  label: statusLabel(s),
                  value: stats.ticketsByStatus[s] ?? 0,
                  to: `/tickets?status=${s}`,
                }))}
              />
            </section>

            <section className="panel">
              <h2>Tickets by category</h2>
              <BarList items={stats.ticketsByCategory.map(c => ({ label: c.category, value: c.count }))} />
            </section>
          </div>

          <section className="panel">
            <h2>Technician workload</h2>
            <table className="table">
              <thead>
                <tr>
                  <th>Technician</th>
                  <th>Status</th>
                  <th>Open tickets</th>
                  <th>Resolved / closed</th>
                </tr>
              </thead>
              <tbody>
                {stats.technicians.map(t => (
                  <tr key={t.id}>
                    <td><Link to={`/tickets?assignedToId=${t.id}`}>{t.fullName}</Link></td>
                    <td>
                      <span className={t.isActive ? 'badge active' : 'badge inactive'}>
                        {t.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td>{t.openTickets}</td>
                    <td>{t.resolvedTickets}</td>
                  </tr>
                ))}
                {stats.technicians.length === 0 && (
                  <tr><td colSpan={4} className="empty">No technicians yet.</td></tr>
                )}
              </tbody>
            </table>
          </section>
        </>
      )}
    </>
  )
}

type StatCardProps = { label: string; value: number | string; warn?: boolean }

function StatCard({ label, value, warn }: StatCardProps) {
  return (
    <div className="panel stat-card">
      <div className={warn ? 'stat-value overdue' : 'stat-value'}>{value}</div>
      <div className="stat-label">{label}</div>
    </div>
  )
}

type BarItem = { label: string; value: number; to?: string }

function BarList({ items }: { items: BarItem[] }) {
  if (items.length === 0) return <p className="muted">No data yet.</p>
  const max = Math.max(1, ...items.map(i => i.value))

  return (
    <ul className="bar-list">
      {items.map(i => (
        <li key={i.label}>
          <span>{i.to ? <Link to={i.to}>{i.label}</Link> : i.label}</span>
          <span className="bar-track">
            <span className="bar" style={{ width: `${(i.value / max) * 100}%` }} />
          </span>
          <span className="bar-value">{i.value}</span>
        </li>
      ))}
    </ul>
  )
}