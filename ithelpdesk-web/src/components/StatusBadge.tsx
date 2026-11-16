import type { TicketStatus } from '../api/types'
import { statusLabel } from '../utils/format'

export default function StatusBadge({ status }: { status: TicketStatus }) {
  return <span className={`badge status-${status.toLowerCase()}`}>{statusLabel(status)}</span>
}