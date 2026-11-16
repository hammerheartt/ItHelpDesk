import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router'
import { useAuth } from '../auth/AuthContext'
import { api, apiPaged, ApiError } from '../api/client'
import {
  TICKET_PRIORITIES, TICKET_STATUSES,
  type Category, type PaginationMetadata, type Ticket, type User,
} from '../api/types'
import StatusBadge from '../components/StatusBadge'
import { formatDate, statusLabel } from '../utils/format'

const PAGE_SIZE = 20

type ListState = {
  query: string
  tickets: Ticket[]
  meta: PaginationMetadata | null
  error: string | null
}

export default function TicketsPage() {
  const { user } = useAuth()
  const role = user!.role
  const [searchParams, setSearchParams] = useSearchParams()
  const [list, setList] = useState<ListState | null>(null)
  const [categories, setCategories] = useState<Category[]>([])
  const [technicians, setTechnicians] = useState<User[]>([])

  const query = searchParams.toString()
  const loading = list?.query !== query
  const meta = list?.meta ?? null

  useEffect(() => {
    api<Category[]>('/categories').then(setCategories).catch(() => {})
    if (role === 'Admin') {
      api<User[]>('/users?role=Technician').then(setTechnicians).catch(() => {})
    }
  }, [role])

  useEffect(() => {
    let ignore = false
    const params = new URLSearchParams(query)
    params.set('pageSize', String(PAGE_SIZE))

    apiPaged<Ticket>('/tickets?' + params.toString())
      .then(result => {
        if (!ignore) setList({ query, tickets: result.items, meta: result.meta, error: null })
      })
      .catch(err => {
        if (!ignore) {
          const message = err instanceof ApiError ? err.message : 'Cannot reach the server.'
          setList({ query, tickets: [], meta: null, error: message })
        }
      })

    return () => { ignore = true }
  }, [query])

  function setFilter(key: string, value: string) {
    const next = new URLSearchParams(searchParams)
    if (value) next.set(key, value)
    else next.delete(key)
    next.delete('pageNumber')
    setSearchParams(next)
  }

  function goToPage(page: number) {
    const next = new URLSearchParams(searchParams)
    next.set('pageNumber', String(page))
    setSearchParams(next)
  }

  const title = role === 'Employee' ? 'My tickets' : role === 'Technician' ? 'Assigned to me' : 'All tickets'
  const showCreatedBy = role !== 'Employee'
  const showAssignedTo = role !== 'Technician'

  return (
    <>
      <div className="page-header">
        <h1>{title}</h1>
        {role === 'Employee' && <Link to="/tickets/new" className="button primary">New ticket</Link>}
      </div>

      <div className="filters">
        <select value={searchParams.get('status') ?? ''} onChange={e => setFilter('status', e.target.value)}>
          <option value="">All statuses</option>
          {TICKET_STATUSES.map(s => <option key={s} value={s}>{statusLabel(s)}</option>)}
        </select>

        <select value={searchParams.get('priority') ?? ''} onChange={e => setFilter('priority', e.target.value)}>
          <option value="">All priorities</option>
          {TICKET_PRIORITIES.map(p => <option key={p} value={p}>{p}</option>)}
        </select>

        <select value={searchParams.get('categoryId') ?? ''} onChange={e => setFilter('categoryId', e.target.value)}>
          <option value="">All categories</option>
          {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>

        {role === 'Admin' && (
          <select value={searchParams.get('assignedToId') ?? ''} onChange={e => setFilter('assignedToId', e.target.value)}>
            <option value="">All technicians</option>
            {technicians.map(t => <option key={t.id} value={t.id}>{t.fullName}</option>)}
          </select>
        )}

        <label className="checkbox">
          <input
            type="checkbox"
            checked={searchParams.get('overdue') === 'true'}
            onChange={e => setFilter('overdue', e.target.checked ? 'true' : '')}
          />
          Overdue only
        </label>
      </div>

      {list?.error && <p className="error">{list.error}</p>}

        {list && (
        <table className={loading ? 'table loading' : 'table'}>
        <thead>
            <tr>
            <th>#</th>
            <th>Title</th>
            <th>Status</th>
            <th>Priority</th>
            <th>Category</th>
            {showCreatedBy && <th>Created by</th>}
            {showAssignedTo && <th>Assigned to</th>}
            <th>Created</th>
            <th>Due</th>
            </tr>
        </thead>
        <tbody>
            {list.tickets.map(t => (
            <tr key={t.id}>
                <td>{t.id}</td>
                <td><Link to={`/tickets/${t.id}`}>{t.title}</Link></td>
                <td><StatusBadge status={t.status} /></td>
                <td>{t.priority ?? '–'}</td>
                <td>{t.categoryName}</td>
                {showCreatedBy && <td>{t.createdByName}</td>}
                {showAssignedTo && <td>{t.assignedToName ?? '–'}</td>}
                <td>{formatDate(t.createdAt)}</td>
                <td className={t.isOverdue ? 'overdue' : ''}>{t.dueAt ? formatDate(t.dueAt) : '–'}</td>
            </tr>
            ))}
            {list.tickets.length === 0 && (
            <tr><td colSpan={9} className="empty">No tickets found.</td></tr>
            )}
        </tbody>
        </table>
      )}

      {meta && meta.totalPages > 1 && (
        <div className="pagination">
          <button disabled={meta.currentPage <= 1} onClick={() => goToPage(meta.currentPage - 1)}>Previous</button>
          <span>Page {meta.currentPage} of {meta.totalPages} · {meta.totalCount} tickets</span>
          <button disabled={meta.currentPage >= meta.totalPages} onClick={() => goToPage(meta.currentPage + 1)}>Next</button>
        </div>
      )}
    </>
  )
}