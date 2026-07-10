/* tslint:disable */
/* eslint-disable */
/**
 * xxx业务应用
 */

/**
 * 单个人员的每日检测统计结果
 *
 * @export
 * @interface LkUserDailyStatOutput
 */
export interface LkUserDailyStatOutput {
    /**
     * 用户ID
     * @type {number}
     * @memberof LkUserDailyStatOutput
     */
    userId?: number;

    /**
     * 用户姓名
     * @type {string}
     * @memberof LkUserDailyStatOutput
     */
    userName?: string | null;

    /**
     * 统计天起始日期，格式 yyyy-MM-dd
     * @type {string}
     * @memberof LkUserDailyStatOutput
     */
    date?: string | null;

    /**
     * 检测总数
     * @type {number}
     * @memberof LkUserDailyStatOutput
     */
    total?: number;

    /**
     * OK 数量
     * @type {number}
     * @memberof LkUserDailyStatOutput
     */
    okCount?: number;

    /**
     * NG 数量
     * @type {number}
     * @memberof LkUserDailyStatOutput
     */
    ngCount?: number;

    /**
     * 合格率（保留4位小数，例如 0.9523 表示 95.23%）
     * @type {number}
     * @memberof LkUserDailyStatOutput
     */
    passRate?: number;
}
