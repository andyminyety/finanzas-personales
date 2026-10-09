import api from './axios'
import type { MonthlyCut, RunCutsResult } from '../types'

export const getCuts = (params: { userId?: string; year?: number }) =>
  api.get<MonthlyCut[]>('/cuts', { params }).then((r) => r.data)

export const runCuts = (userId?: string) =>
  api.post<RunCutsResult>('/cuts/run', { userId: userId || null }).then((r) => r.data)
