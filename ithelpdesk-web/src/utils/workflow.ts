import type { Role } from "../auth/jwt";
import type { Ticket, TicketPriority, TicketStatus } from "../api/types";

const transitions: { from: TicketStatus; to: TicketStatus; roles: Role[] }[] = [
  { from: 'New', to: 'Cancelled', roles: ['Employee'] },
  { from: 'Assigned', to: 'InProgress', roles: ['Technician'] },
  { from: 'InProgress', to: 'Resolved', roles: ['Technician'] },
  { from: 'Resolved', to: 'Closed', roles: ['Employee', 'Admin'] },
  { from: 'Resolved', to: 'Reopened', roles: ['Employee'] },
  { from: 'Reopened', to: 'InProgress', roles: ['Technician'] },
]

const REOPEN_WINDOW_MS = 7 * 24 * 60 * 60 * 1000

export const OPEN_STATUSES: TicketStatus[] = ['Assigned', 'InProgress', 'Reopened']

export const DEADLINE_LABELS: Record<TicketPriority, string> = {
  Critical: '4 hours',
  High: '1 day',
  Medium: '3 days',
  Low: '5 days',
}

export function allowedTransitions(ticket: Ticket, role: Role): TicketStatus[] {
  return transitions
    .filter(t => t.from === ticket.status && t.roles.includes(role))
    .map(t => t.to)
    .filter(to => to !== 'Reopened' || withinReopenWindow(ticket))
}

export function canAssign(ticket: Ticket): boolean {
  return ticket.status === 'New' || OPEN_STATUSES.includes(ticket.status)
}

function withinReopenWindow(ticket: Ticket): boolean {
  return ticket.resolvedAt !== null && Date.now() - Date.parse(ticket.resolvedAt) <= REOPEN_WINDOW_MS
}