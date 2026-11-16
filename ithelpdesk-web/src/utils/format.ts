import type { TicketStatus } from '../api/types'

export function formatDate(value: string): string {
  return new Date(value).toLocaleString('en-GB', { dateStyle: 'short', timeStyle: 'short' })
}

export function statusLabel(status: TicketStatus): string {
  return status === 'InProgress' ? 'In progress' : status
}