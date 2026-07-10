/* tslint:disable */
/* eslint-disable */
/**
 * xxx业务应用
 */

import { LkProductTypeDailyStatOutput } from './lk-product-type-daily-stat-output';

/**
 * 全局返回结果
 *
 * @export
 * @interface AdminResultListLkProductTypeDailyStatOutput
 */
export interface AdminResultListLkProductTypeDailyStatOutput {
    /** 状态码 */
    code?: number;
    /** 类型 success、warning、error */
    type?: string | null;
    /** 错误信息 */
    message?: string | null;
    /** 数据 */
    result?: Array<LkProductTypeDailyStatOutput> | null;
    /** 附加数据 */
    extras?: any | null;
    /** 时间 */
    time?: Date;
}
