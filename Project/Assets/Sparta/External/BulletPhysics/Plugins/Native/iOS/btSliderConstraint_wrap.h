#include "main.h"

extern "C"
{
	EXPORT btSliderConstraint* btSliderConstraint_new(btRigidBody* rbA, btRigidBody* rbB, const btScalar* frameInA, const btScalar* frameInB, bool useLinearReferenceFrameA);
	EXPORT btSliderConstraint* btSliderConstraint_new2(btRigidBody* rbB, const btScalar* frameInB, bool useLinearReferenceFrameA);
	EXPORT void btSliderConstraint_calculateTransforms(btSliderConstraint* obj, const btScalar* transA, const btScalar* transB);
	EXPORT void btSliderConstraint_getAncorInA(btSliderConstraint* obj, btScalar* value);
	EXPORT void btSliderConstraint_getAncorInB(btSliderConstraint* obj, btScalar* value);
	EXPORT btScalar btSliderConstraint_getAngDepth(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getAngularPos(btSliderConstraint* obj);
	EXPORT void btSliderConstraint_getCalculatedTransformA(btSliderConstraint* obj, btScalar* value);
	EXPORT void btSliderConstraint_getCalculatedTransformB(btSliderConstraint* obj, btScalar* value);
	EXPORT btScalar btSliderConstraint_getDampingDirAng(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getDampingDirLin(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getDampingLimAng(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getDampingLimLin(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getDampingOrthoAng(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getDampingOrthoLin(btSliderConstraint* obj);
	EXPORT int btSliderConstraint_getFlags(btSliderConstraint* obj);
	EXPORT void btSliderConstraint_getFrameOffsetA(btSliderConstraint* obj, btScalar* value);
	EXPORT void btSliderConstraint_getFrameOffsetB(btSliderConstraint* obj, btScalar* value);
	EXPORT void btSliderConstraint_getInfo1NonVirtual(btSliderConstraint* obj, btTypedConstraint_btConstraintInfo1* info);
	EXPORT void btSliderConstraint_getInfo2NonVirtual(btSliderConstraint* obj, btTypedConstraint_btConstraintInfo2* info, const btScalar* transA, const btScalar* transB, const btScalar* linVelA, const btScalar* linVelB, btScalar rbAinvMass, btScalar rbBinvMass);
	EXPORT btScalar btSliderConstraint_getLinDepth(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getLinearPos(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getLowerAngLimit(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getLowerLinLimit(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getMaxAngMotorForce(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getMaxLinMotorForce(btSliderConstraint* obj);
	EXPORT bool btSliderConstraint_getPoweredAngMotor(btSliderConstraint* obj);
	EXPORT bool btSliderConstraint_getPoweredLinMotor(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getRestitutionDirAng(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getRestitutionDirLin(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getRestitutionLimAng(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getRestitutionLimLin(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getRestitutionOrthoAng(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getRestitutionOrthoLin(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getSoftnessDirAng(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getSoftnessDirLin(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getSoftnessLimAng(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getSoftnessLimLin(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getSoftnessOrthoAng(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getSoftnessOrthoLin(btSliderConstraint* obj);
	EXPORT bool btSliderConstraint_getSolveAngLimit(btSliderConstraint* obj);
	EXPORT bool btSliderConstraint_getSolveLinLimit(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getTargetAngMotorVelocity(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getTargetLinMotorVelocity(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getUpperAngLimit(btSliderConstraint* obj);
	EXPORT btScalar btSliderConstraint_getUpperLinLimit(btSliderConstraint* obj);
	EXPORT bool btSliderConstraint_getUseFrameOffset(btSliderConstraint* obj);
	EXPORT bool btSliderConstraint_getUseLinearReferenceFrameA(btSliderConstraint* obj);
	EXPORT void btSliderConstraint_setDampingDirAng(btSliderConstraint* obj, btScalar dampingDirAng);
	EXPORT void btSliderConstraint_setDampingDirLin(btSliderConstraint* obj, btScalar dampingDirLin);
	EXPORT void btSliderConstraint_setDampingLimAng(btSliderConstraint* obj, btScalar dampingLimAng);
	EXPORT void btSliderConstraint_setDampingLimLin(btSliderConstraint* obj, btScalar dampingLimLin);
	EXPORT void btSliderConstraint_setDampingOrthoAng(btSliderConstraint* obj, btScalar dampingOrthoAng);
	EXPORT void btSliderConstraint_setDampingOrthoLin(btSliderConstraint* obj, btScalar dampingOrthoLin);
	EXPORT void btSliderConstraint_setFrames(btSliderConstraint* obj, const btScalar* frameA, const btScalar* frameB);
	EXPORT void btSliderConstraint_setLowerAngLimit(btSliderConstraint* obj, btScalar lowerLimit);
	EXPORT void btSliderConstraint_setLowerLinLimit(btSliderConstraint* obj, btScalar lowerLimit);
	EXPORT void btSliderConstraint_setMaxAngMotorForce(btSliderConstraint* obj, btScalar maxAngMotorForce);
	EXPORT void btSliderConstraint_setMaxLinMotorForce(btSliderConstraint* obj, btScalar maxLinMotorForce);
	EXPORT void btSliderConstraint_setPoweredAngMotor(btSliderConstraint* obj, bool onOff);
	EXPORT void btSliderConstraint_setPoweredLinMotor(btSliderConstraint* obj, bool onOff);
	EXPORT void btSliderConstraint_setRestitutionDirAng(btSliderConstraint* obj, btScalar restitutionDirAng);
	EXPORT void btSliderConstraint_setRestitutionDirLin(btSliderConstraint* obj, btScalar restitutionDirLin);
	EXPORT void btSliderConstraint_setRestitutionLimAng(btSliderConstraint* obj, btScalar restitutionLimAng);
	EXPORT void btSliderConstraint_setRestitutionLimLin(btSliderConstraint* obj, btScalar restitutionLimLin);
	EXPORT void btSliderConstraint_setRestitutionOrthoAng(btSliderConstraint* obj, btScalar restitutionOrthoAng);
	EXPORT void btSliderConstraint_setRestitutionOrthoLin(btSliderConstraint* obj, btScalar restitutionOrthoLin);
	EXPORT void btSliderConstraint_setSoftnessDirAng(btSliderConstraint* obj, btScalar softnessDirAng);
	EXPORT void btSliderConstraint_setSoftnessDirLin(btSliderConstraint* obj, btScalar softnessDirLin);
	EXPORT void btSliderConstraint_setSoftnessLimAng(btSliderConstraint* obj, btScalar softnessLimAng);
	EXPORT void btSliderConstraint_setSoftnessLimLin(btSliderConstraint* obj, btScalar softnessLimLin);
	EXPORT void btSliderConstraint_setSoftnessOrthoAng(btSliderConstraint* obj, btScalar softnessOrthoAng);
	EXPORT void btSliderConstraint_setSoftnessOrthoLin(btSliderConstraint* obj, btScalar softnessOrthoLin);
	EXPORT void btSliderConstraint_setTargetAngMotorVelocity(btSliderConstraint* obj, btScalar targetAngMotorVelocity);
	EXPORT void btSliderConstraint_setTargetLinMotorVelocity(btSliderConstraint* obj, btScalar targetLinMotorVelocity);
	EXPORT void btSliderConstraint_setUpperAngLimit(btSliderConstraint* obj, btScalar upperLimit);
	EXPORT void btSliderConstraint_setUpperLinLimit(btSliderConstraint* obj, btScalar upperLimit);
	EXPORT void btSliderConstraint_setUseFrameOffset(btSliderConstraint* obj, bool frameOffsetOnOff);
	EXPORT void btSliderConstraint_testAngLimits(btSliderConstraint* obj);
	EXPORT void btSliderConstraint_testLinLimits(btSliderConstraint* obj);
}
