import type { TicketStatus } from '../api/types'

export function formatDate(value: string): string {
  return new Date(value).toLocaleString('en-GB', { dateStyle: 'short', timeStyle: 'short' })
}

export function statusLabel(status: TicketStatus): string {
  return status === 'InProgress' ? 'In progress' : status
}

export function formatHours(hours: number | null): string {
  if (hours === null) return '–'
  return hours < 24 ? `${hours.toFixed(1)} h` : `${(hours / 24).toFixed(1)} days`
}