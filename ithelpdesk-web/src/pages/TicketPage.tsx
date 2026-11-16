import { useEffect, useState, type ReactNode } from 'react'
import { useParams } from 'react-router'
import { useAuth } from '../auth/AuthContext'
import { api, ApiError } from '../api/client'
import type { Comment, HistoryEntry, Ticket, TicketStatus } from '../api/types'
import StatusBadge from '../components/StatusBadge'
import CommentForm from '../components/CommentForm'
import TicketActions from '../components/TicketActions'
import AssignForm from '../components/AssignForm'
import NotFoundPage from './NotFoundPage'
import { formatDate, statusLabel } from '../utils/format'
import { canAssign } from '../utils/workflow'

type PageData = {
  ticketId: number
  ticket: Ticket
  comments: Comment[]
  history: HistoryEntry[]
}

type LoadError = {
  ticketId: number
  status: number
  message: string
}

export default function TicketPage() {
  const { id } = useParams()
  const { user } = useAuth()
  const ticketId = Number(id)
  const validId = Number.isInteger(ticketId) && ticketId > 0

  const [data, setData] = useState<PageData | null>(null)
  const [loadError, setLoadError] = useState<LoadError | null>(null)
  const [version, setVersion] = useState(0)

  useEffect(() => {
    if (!validId) return
    let ignore = false

    Promise.all([
      api<Ticket>(`/tickets/${ticketId}`),
      api<Comment[]>(`/tickets/${ticketId}/comments`),
      api<HistoryEntry[]>(`/tickets/${ticketId}/history`),
    ])
      .then(([ticket, comments, history]) => {
        if (!ignore) setData({ ticketId, ticket, comments, history })
      })
      .catch(err => {
        if (!ignore) {
          setLoadError({
            ticketId,
            status: err instanceof ApiError ? err.status : 0,
            message: err instanceof ApiError ? err.message : 'Cannot reach the server.',
          })
        }
      })

    return () => { ignore = true }
  }, [ticketId, validId, version])

  if (!validId) return <NotFoundPage />
  if (loadError?.ticketId === ticketId) {
    return loadError.status === 404 ? <NotFoundPage /> : <p className="error">{loadError.message}</p>
  }
  if (data?.ticketId !== ticketId) return null

  const { ticket, comments, history } = data
  const role = user!.role
  const canWriteInternal = role !== 'Employee'
  const isFinal = ticket.status === 'Closed' || ticket.status === 'Cancelled'

  function addComment(comment: Comment) {
    setData(current => current && { ...current, comments: [...current.comments, comment] })
  }

  function reload() {
    setVersion(v => v + 1)
  }

  return (
    <>
      <div className="ticket-header">
        <h1>#{ticket.id} {ticket.title}</h1>
        <StatusBadge status={ticket.status} />
      </div>

      <div className="ticket-layout">
        <section className="panel history-panel">
          <h2>History</h2>
          <ul className="history">
            {[...history].reverse().map(h => (
              <li key={h.id}>
                <strong>{h.changedByName}</strong> {describeHistory(h)}
                <time>{formatDate(h.changedAt)}</time>
              </li>
            ))}
          </ul>
        </section>

        <div className="main-column">
          <section className="panel description-panel">
            <h2>Description</h2>
            <p className="pre">{ticket.description}</p>
            {ticket.resolutionNote && (
              <div className="resolution">
                <strong>Resolution</strong>
                <p className="pre">{ticket.resolutionNote}</p>
              </div>
            )}
          </section>

          <section className="panel comments-panel">
            <h2>Comments</h2>
            <ul className="comments">
                {comments.length === 0 && <li className="muted">No comments yet.</li>}
                {[...comments].reverse().map(c => (
                <li key={c.id} className={c.isInternal ? 'comment internal' : 'comment'}>
                    <div className="comment-meta">
                    <strong>{c.authorName}</strong> {formatDate(c.createdAt)}
                    {c.isInternal && <span className="badge internal-badge">Internal</span>}
                    </div>
                    <p className="pre">{c.text}</p>
                </li>
                ))}
            </ul>
            {isFinal ? (
                <p className="muted">This ticket is {ticket.status.toLowerCase()}. Comments are closed.</p>
            ) : (
                <CommentForm ticketId={ticket.id} canWriteInternal={canWriteInternal} onAdded={addComment} />
            )}
            </section>
        </div>

        <div className="stack">
          <TicketActions ticket={ticket} role={role} onChanged={reload} />
          {role === 'Admin' && canAssign(ticket) && <AssignForm ticket={ticket} onChanged={reload} />}

          <section className="panel">
            <h2>Details</h2>
            <dl className="details">
              <Row label="Priority">{ticket.priority ?? '–'}</Row>
              <Row label="Category">{ticket.categoryName}</Row>
              <Row label="Created by">{ticket.createdByName}</Row>
              <Row label="Assigned to">{ticket.assignedToName ?? '–'}</Row>
              <Row label="Created">{formatDate(ticket.createdAt)}</Row>
              <Row label="Due">
                {ticket.dueAt ? (
                  <span className={ticket.isOverdue ? 'overdue' : ''}>
                    {formatDate(ticket.dueAt)}{ticket.isOverdue && ' (overdue)'}
                  </span>
                ) : '–'}
              </Row>
              {ticket.resolvedAt && <Row label="Resolved">{formatDate(ticket.resolvedAt)}</Row>}
              {ticket.closedAt && <Row label="Closed">{formatDate(ticket.closedAt)}</Row>}
              {ticket.reopenCount > 0 && <Row label="Reopened">{ticket.reopenCount}×</Row>}
            </dl>
          </section>
        </div>
      </div>
    </>
  )
}

function Row({ label, children }: { label: string; children: ReactNode }) {
  return (
    <>
      <dt>{label}</dt>
      <dd>{children}</dd>
    </>
  )
}

function describeHistory(h: HistoryEntry): string {
  switch (h.action) {
    case 'Created':
      return 'created the ticket'
    case 'Assigned':
      return h.oldValue ? `reassigned it from ${h.oldValue} to ${h.newValue}` : `assigned it to ${h.newValue}`
    case 'StatusChanged':
      return `changed status from ${statusLabel(h.oldValue as TicketStatus)} to ${statusLabel(h.newValue as TicketStatus)}`
    default:
      return h.action
  }
}